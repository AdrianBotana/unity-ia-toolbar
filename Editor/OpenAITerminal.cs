using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public enum AITool
{
    Claude,
    Copilot
}

[InitializeOnLoad]
public static class OpenAITerminal
{
    private const string PrefKey = "AIToolSelection";

    private static bool initialized;
    private static int attempts;
    private static TextElement buttonLabel;
    private static Image buttonIcon;
    private static ToolbarButton toolbarButton;

    public static AITool SelectedTool
    {
        get => (AITool)EditorPrefs.GetInt(PrefKey, (int)AITool.Claude);
        set
        {
            EditorPrefs.SetInt(PrefKey, (int)value);
            UpdateButtonAppearance();
        }
    }

    static OpenAITerminal()
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

        toolbarButton = new ToolbarButton(OpenTerminal);
        toolbarButton.text = "";
        toolbarButton.style.flexDirection = FlexDirection.Row;
        toolbarButton.style.alignItems = Align.Center;
        toolbarButton.style.paddingLeft = 6;
        toolbarButton.style.paddingRight = 6;

        buttonIcon = new Image();
        buttonIcon.style.width = 14;
        buttonIcon.style.height = 14;
        buttonIcon.style.marginRight = 4;
        toolbarButton.Add(buttonIcon);

        buttonLabel = new TextElement();
        buttonLabel.style.fontSize = 11;
        toolbarButton.Add(buttonLabel);

        UpdateButtonAppearance();

        var parent = playZone.parent;
        int playIndex = parent.IndexOf(playZone);
        parent.Insert(playIndex + 1, toolbarButton);

        initialized = true;
        EditorApplication.update -= TryInitialize;
    }

    private static void UpdateButtonAppearance()
    {
        if (buttonLabel == null || buttonIcon == null || toolbarButton == null) return;

        var tool = SelectedTool;
        buttonLabel.text = tool == AITool.Claude ? "Claude" : "Copilot";
        buttonIcon.image = tool == AITool.Claude ? CreateClaudeIcon() : CreateCopilotIcon();
        toolbarButton.tooltip = tool == AITool.Claude
            ? "Open Claude Code terminal in project directory"
            : "Open GitHub Copilot chat in project directory";
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

    private static Texture2D CreateCopilotIcon()
    {
        int size = 16;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;

        var pixels = new Color[size * size];
        var clear = new Color(0, 0, 0, 0);
        var blue = new Color(0.24f, 0.54f, 0.96f, 1f);
        var blueBright = new Color(0.40f, 0.68f, 1f, 1f);

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = clear;

        float cx = 7.5f;
        float cy = 7.5f;

        // Draw two overlapping circles to represent Copilot's twin-lens look
        float[] offsetsX = { -2.5f, 2.5f };
        foreach (float offX in offsetsX)
        {
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Mathf.Sqrt((x - (cx + offX)) * (x - (cx + offX)) + (y - cy) * (y - cy));
                    if (dist < 5f)
                    {
                        float alpha = Mathf.Clamp01(1f - dist / 5f);
                        alpha = Mathf.Pow(alpha, 0.6f);
                        var c = Color.Lerp(blue, blueBright, alpha);
                        c.a = Mathf.Max(pixels[y * size + x].a, alpha);
                        pixels[y * size + x] = c;
                    }
                }
            }
        }

        // Center visor line
        for (int x = 3; x <= 12; x++)
        {
            for (int y = 6; y <= 9; y++)
            {
                float dist = Mathf.Abs(y - 7.5f);
                if (dist < 1.5f)
                {
                    float alpha = 1f - dist / 1.5f;
                    int idx = y * size + x;
                    var c = blueBright;
                    c.a = Mathf.Max(pixels[idx].a, alpha * 0.9f);
                    pixels[idx] = c;
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    [MenuItem("Tools/Open AI Terminal")]
    public static void OpenTerminal()
    {
        string projectPath = Application.dataPath.Replace("/Assets", "");
        string command = SelectedTool == AITool.Claude
            ? "claude --dangerously-skip-permissions"
            : "copilot";

#if UNITY_EDITOR_WIN
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "cmd.exe";
        process.StartInfo.Arguments = $"/k cd /d \"{projectPath}\" && {command}";
        process.StartInfo.UseShellExecute = true;
        process.Start();
#elif UNITY_EDITOR_OSX
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c 'cd \"{projectPath}\" && {command}'";
        process.StartInfo.UseShellExecute = true;
        process.Start();
#elif UNITY_EDITOR_LINUX
        var process = new System.Diagnostics.Process();
        process.StartInfo.FileName = "/bin/bash";
        process.StartInfo.Arguments = $"-c 'cd \"{projectPath}\" && {command}'";
        process.StartInfo.UseShellExecute = true;
        process.Start();
#endif
    }
}

public class AIToolSettingsProvider : SettingsProvider
{
    public AIToolSettingsProvider() : base("Preferences/AI Tool", SettingsScope.User) { }

    public override void OnGUI(string searchContext)
    {
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("AI Tool Configuration", EditorStyles.boldLabel);
        EditorGUILayout.Space(5);

        var current = OpenAITerminal.SelectedTool;
        var selected = (AITool)EditorGUILayout.EnumPopup("Active AI Tool", current);

        if (selected != current)
        {
            OpenAITerminal.SelectedTool = selected;
        }

        EditorGUILayout.Space(10);
        EditorGUILayout.HelpBox(
            selected == AITool.Claude
                ? "Claude Code will open in a terminal with --dangerously-skip-permissions flag."
                : "GitHub Copilot CLI (copilot) will open in a terminal.",
            MessageType.Info);
    }

    [SettingsProvider]
    public static SettingsProvider CreateProvider()
    {
        return new AIToolSettingsProvider
        {
            keywords = new[] { "AI", "Claude", "Copilot", "Tool", "Terminal" }
        };
    }
}
