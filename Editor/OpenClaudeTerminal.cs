using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

[InitializeOnLoad]
public static class OpenClaudeTerminal
{
    private static bool initialized;
    private static int attempts;

    static OpenClaudeTerminal()
    {
        EditorApplication.update += TryInitialize;
    }

    private static void TryInitialize()
    {
        if (initialized) return;
        attempts++;
        if (attempts > 300)
        {
            EditorApplication.update -= TryInitialize;
            return;
        }

        var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
        if (toolbarType == null) return;

        var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
        if (toolbars.Length == 0) return;

        var toolbar = toolbars[0];

        var visualTreeProp = toolbar.GetType().GetProperty("visualTree",
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        if (visualTreeProp == null) return;

        var root = visualTreeProp.GetValue(toolbar) as VisualElement;
        if (root == null) return;

        var playZone = root.Q("ToolbarZonePlayMode");
        if (playZone == null) return;

        var icon = CreateClaudeIcon();

        var button = new ToolbarButton(OpenTerminal);
        button.text = "";
        button.tooltip = "Open Claude Code terminal in project directory";
        button.style.flexDirection = FlexDirection.Row;
        button.style.alignItems = Align.Center;
        button.style.paddingLeft = 6;
        button.style.paddingRight = 6;

        var iconElement = new Image();
        iconElement.image = icon;
        iconElement.style.width = 14;
        iconElement.style.height = 14;
        iconElement.style.marginRight = 4;
        button.Add(iconElement);

        var label = new TextElement();
        label.text = "Claude";
        label.style.fontSize = 11;
        button.Add(label);

        var parent = playZone.parent;
        int playIndex = parent.IndexOf(playZone);
        parent.Insert(playIndex + 1, button);

        initialized = true;
        EditorApplication.update -= TryInitialize;
    }

    private static Texture2D CreateClaudeIcon()
    {
        int size = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        var pixels = new Color[size * size];
        var clear = new Color(0, 0, 0, 0);
        var coral = new Color(0.90f, 0.50f, 0.25f, 1f);
        var coralBright = new Color(0.95f, 0.60f, 0.35f, 1f);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        float cx = 7.5f;
        float cy = 7.5f;

        for (int a = 0; a < 6; a++)
        {
            float angle = a * 60f * Mathf.Deg2Rad;
            float dx = Mathf.Cos(angle);
            float dy = Mathf.Sin(angle);

            for (float t = 0; t < 6.5f; t += 0.25f)
            {
                float px = cx + dx * t;
                float py = cy + dy * t;

                for (int ox = -1; ox <= 1; ox++)
                {
                    for (int oy = -1; oy <= 1; oy++)
                    {
                        if (ox != 0 && oy != 0) continue;
                        int ix = Mathf.RoundToInt(px) + ox;
                        int iy = Mathf.RoundToInt(py) + oy;

                        if (ix < 0 || ix >= size || iy < 0 || iy >= size) continue;

                        float fade = 1f - (t / 6.5f) * 0.6f;
                        float edgeFade = (ox == 0 && oy == 0) ? 1f : 0.4f;
                        float alpha = fade * edgeFade;

                        var existing = pixels[iy * size + ix];
                        if (existing.a < alpha)
                        {
                            var c = Color.Lerp(coral, coralBright, 1f - fade);
                            c.a = alpha;
                            pixels[iy * size + ix] = c;
                        }
                    }
                }
            }
        }

        for (int y = 5; y <= 10; y++)
        {
            for (int x = 5; x <= 10; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                if (dist < 2.5f)
                {
                    float alpha = Mathf.Clamp01(1f - dist / 2.5f);
                    alpha = alpha * alpha;
                    var c = new Color(0.95f, 0.60f, 0.35f, Mathf.Max(pixels[y * size + x].a, alpha));
                    pixels[y * size + x] = c;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    [MenuItem("Tools/Open Claude Terminal")]
    public static void OpenTerminal()
    {
        string projectPath = Application.dataPath.Replace("/Assets", "");

#if UNITY_EDITOR_WIN
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/k cd /d \"{projectPath}\" && claude --dangerously-skip-permissions";
        process.StartInfo.UseShellExecute = true;
        process.Start();
#elif UNITY_EDITOR_OSX
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c 'cd \"{projectPath}\" && claude --dangerously-skip-permissions'";
        process.StartInfo.UseShellExecute = true;
        process.Start();
#elif UNITY_EDITOR_LINUX
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c 'cd \"{projectPath}\" && claude --dangerously-skip-permissions'";
        process.StartInfo.UseShellExecute = true;
        process.Start();
#endif
    }
}
