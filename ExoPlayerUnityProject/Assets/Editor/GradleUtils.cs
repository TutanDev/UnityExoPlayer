using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class GradleUtils 
{
    [MenuItem("Tools/Enable Native Android Video Player")]
    public static void EnableNativeVideoPlayer()
    {
        // Enable gradle build with exoplayer
        UseGradle();
        var lines = ReadLines();
        AddDependency("com.google.android.exoplayer:exoplayer", "2.18.1", lines);
        WriteLines(lines);
    }

    private static readonly string androidPluginsFolder = "Assets/Plugins/Android/";
    private static readonly string gradleTemplatePath = androidPluginsFolder + "mainTemplate.gradle";
    private static readonly string disabledGradleTemplatePath = gradleTemplatePath + ".DISABLED";
    private static readonly string internalGradleTemplatePath = Path.Combine(Path.Combine(GetBuildToolsDirectory(BuildTarget.Android), "GradleTemplates"), "mainTemplate.gradle");

    private static string GetBuildToolsDirectory(UnityEditor.BuildTarget bt)
    {
        return (string)(typeof(BuildPipeline).GetMethod("GetBuildToolsDirectory", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic).Invoke(null, new object[] { bt }));
    }

    public static void UseGradle()
    {
        EditorUserBuildSettings.androidBuildSystem = AndroidBuildSystem.Gradle;

        // create android plugins directory if it doesn't exist
        if (!Directory.Exists(androidPluginsFolder))
        {
            Directory.CreateDirectory(androidPluginsFolder);
        }

        if (!File.Exists(gradleTemplatePath))
        {
            if (File.Exists(gradleTemplatePath + ".DISABLED"))
            {
                File.Move(disabledGradleTemplatePath, gradleTemplatePath);
                File.Move(disabledGradleTemplatePath + ".meta", gradleTemplatePath + ".meta");
            }
            else
            {
                File.Copy(internalGradleTemplatePath, gradleTemplatePath);
            }
            AssetDatabase.ImportAsset(gradleTemplatePath);
        }
    }

    public static List<string> ReadLines()
    {
        var allText = IsUsingGradle() ? File.ReadAllText(gradleTemplatePath) : "";
        return new List<string>(allText.Split('\n'));
    }

    public static void AddDependency(string name, string version, List<string> lines)
    {
        int dependencies = GoToSection("dependencies", lines);
        if (FindInScope(Regex.Escape(name), dependencies + 1, lines) == -1)
        {
            lines.Insert(GetScopeEnd(dependencies + 1, lines), $"\tcompile '{name}:{version}'");
        }
    }

    public static void WriteLines(List<string> lines)
    {
        if (IsUsingGradle())
        {
            File.WriteAllText(gradleTemplatePath, string.Join("\n", lines.ToArray()));
        }
    }

    public static int GoToSection(string section, List<string> lines)
    {
        return GoToSection(section, 0, lines);
    }

    public static int GoToSection(string section, int start, List<string> lines)
    {
        var sections = section.Split('.');

        int p = start - 1;
        for (int i = 0; i < sections.Length; i++)
        {
            p = FindInScope("\\s*" + sections[i] + "\\s*\\{\\s*", p + 1, lines);
        }

        return p;
    }

    public static int FindInScope(string search, int start, List<string> lines)
    {
        var regex = new System.Text.RegularExpressions.Regex(search);

        int depth = 0;

        for (int i = start; i < lines.Count; i++)
        {
            if (depth == 0 && regex.IsMatch(lines[i]))
            {
                return i;
            }

            // count the number of open and close braces. If we leave the current scope, break
            if (lines[i].Contains("{"))
            {
                depth++;
            }
            if (lines[i].Contains("}"))
            {
                depth--;
            }
            if (depth < 0)
            {
                break;
            }
        }
        return -1;
    }

    public static int GetScopeEnd(int start, List<string> lines)
    {
        int depth = 0;
        for (int i = start; i < lines.Count; i++)
        {
            // count the number of open and close braces. If we leave the current scope, break
            if (lines[i].Contains("{"))
            {
                depth++;
            }
            if (lines[i].Contains("}"))
            {
                depth--;
            }
            if (depth < 0)
            {
                return i;
            }
        }

        return -1;
    }

    public static bool IsUsingGradle()
    {
        return EditorUserBuildSettings.androidBuildSystem == AndroidBuildSystem.Gradle
            && Directory.Exists(androidPluginsFolder)
            && File.Exists(gradleTemplatePath);
    }

}
