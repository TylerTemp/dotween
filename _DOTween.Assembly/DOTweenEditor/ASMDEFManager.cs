﻿// Author: Daniele Giardini - http://www.demigiant.com
// Created: 2019/03/05 12:37
// License Copyright (c) Daniele Giardini
// This work is subject to the terms at http://dotween.demigiant.com/license.php

using System;
using System.IO;
using DG.DOTweenEditor.UI;
using DG.Tweening.Core;
using UnityEditor;
using UnityEngine;

namespace DG.DOTweenEditor
{
    internal static class ASMDEFManager
    {
        public enum ASMDEFType
        {
            Modules,
            DOTweenPro,
            DOTweenProEditor,
            DOTweenTimeline,
            DOTweenTimelineEditor
        }

        enum ChangeType
        {
            Deleted,
            Created,
            Overwritten
        }

        public static bool hasModulesASMDEF { get; private set; }
        public static bool hasProASMDEF { get; private set; }
        public static bool hasProEditorASMDEF { get; private set; }
        public static bool hasDOTweenTimelineASMDEF { get; private set; }
        public static bool hasDOTweenTimelineEditorASMDEF { get; private set; }


        const string _ModulesId = "DOTween.Modules";
        const string _ProId = "DOTweenPro.Scripts";
        const string _ProEditorId = "DOTweenPro.EditorScripts";
        const string _DOTweenTimelineId = "DOTweenTimeline.Scripts";
        const string _DOTweenTimelineEditorId = "DOTweenTimeline.EditorScripts";
        const string _ModulesASMDEFFile = _ModulesId + ".asmdef";
        const string _ProASMDEFFile = _ProId + ".asmdef";
        const string _ProEditorASMDEFFile = _ProEditorId + ".asmdef";
        const string _DOTweenTimelineASMDEFFile = _DOTweenTimelineId + ".asmdef";
        const string _DOTweenTimelineEditorASMDEFFile = _DOTweenTimelineEditorId + ".asmdef";

        const string _RefTextMeshPro = "Unity.TextMeshPro";

        static ASMDEFManager()
        {
            Refresh();
        }

        #region Public Methods

        // Also called via Reflection by Autorun
        public static void ApplyASMDEFSettings()
        {
            Refresh();
            DOTweenSettings src = DOTweenUtilityWindow.GetDOTweenSettings();
            if (src != null) {
                if (src.createASMDEF) CreateAllASMDEF();
                else RemoveAllASMDEF();
            }
            RefreshExistingASMDEFFiles();
        }

        public static void Refresh()
        {
            hasModulesASMDEF = File.Exists(EditorUtils.dotweenModulesDir + _ModulesASMDEFFile);
            hasProASMDEF = File.Exists(EditorUtils.dotweenProDir + _ProASMDEFFile);
            hasProEditorASMDEF = File.Exists(EditorUtils.dotweenProEditorDir + _ProEditorASMDEFFile);
            hasDOTweenTimelineASMDEF = File.Exists(EditorUtils.dotweenTimelineScriptsDir + _DOTweenTimelineASMDEFFile);
            hasDOTweenTimelineEditorASMDEF = File.Exists(EditorUtils.dotweenTimelineEditorScriptsDir + _DOTweenTimelineEditorASMDEFFile);
        }

        public static void RefreshExistingASMDEFFiles()
        {
            Refresh();

            // if (!hasModulesASMDEF) {
            //     if (hasProASMDEF) RemoveASMDEF(ASMDEFType.DOTweenPro);
            //     if (hasProEditorASMDEF) RemoveASMDEF(ASMDEFType.DOTweenProEditor);
            //     if (hasDOTweenTimelineASMDEF) RemoveASMDEF(ASMDEFType.DOTweenTimeline);
            //     if (hasDOTweenTimelineEditorASMDEF) RemoveASMDEF(ASMDEFType.DOTweenTimelineEditor);
            //     return;
            // }

            DOTweenSettings src = DOTweenUtilityWindow.GetDOTweenSettings();
            if (src == null || !src.createASMDEF) return;
            if (EditorUtils.hasPro) {
                // if (!hasProASMDEF) CreateASMDEF(ASMDEFType.DOTweenPro);
                // if (!hasProEditorASMDEF) CreateASMDEF(ASMDEFType.DOTweenProEditor);
                // Pro ASMDEF present: check that they contain correct elements
                ValidateProASMDEFReferences(src, ASMDEFType.DOTweenPro, EditorUtils.dotweenProDir + _ProASMDEFFile);
                ValidateProASMDEFReferences(src, ASMDEFType.DOTweenProEditor, EditorUtils.dotweenProEditorDir + _ProEditorASMDEFFile);
            }
            if (EditorUtils.hasDOTweenTimeline) {
                // if (!hasDOTweenTimelineASMDEF) CreateASMDEF(ASMDEFType.DOTweenTimeline);
                // if (!hasDOTweenTimelineEditorASMDEF) CreateASMDEF(ASMDEFType.DOTweenTimelineEditor);
                // Timeline ASMDEF present: check that they contain correct elements
                ValidateDOTweenTimelineASMDEFReferences(src, ASMDEFType.DOTweenTimeline, EditorUtils.dotweenTimelineScriptsDir + _DOTweenTimelineASMDEFFile);
                ValidateDOTweenTimelineASMDEFReferences(src, ASMDEFType.DOTweenTimelineEditor, EditorUtils.dotweenTimelineEditorScriptsDir + _DOTweenTimelineEditorASMDEFFile);
            }
        }

        public static void CreateAllASMDEF()
        {
            DOTweenSettings src = DOTweenUtilityWindow.GetDOTweenSettings();
            src.createASMDEF = true;
            EditorUtility.SetDirty(src);
            if (!hasModulesASMDEF) CreateASMDEF(ASMDEFType.Modules);
            if (EditorUtils.hasPro) {
                if (!hasProASMDEF) CreateASMDEF(ASMDEFType.DOTweenPro);
                if (!hasProEditorASMDEF) CreateASMDEF(ASMDEFType.DOTweenProEditor);
            }
            if (EditorUtils.hasDOTweenTimeline) {
                if (!hasDOTweenTimelineASMDEF) CreateASMDEF(ASMDEFType.DOTweenTimeline);
                if (!hasDOTweenTimelineEditorASMDEF) CreateASMDEF(ASMDEFType.DOTweenTimelineEditor);
            }

            if (!EditorUtils.HasGlobalDefine(DOTweenDefines.GlobalDefine_DOTween_ASDMEF))
                EditorUtils.AddGlobalDefine(DOTweenDefines.GlobalDefine_DOTween_ASDMEF);
        }

        public static void RemoveAllASMDEF()
        {
            DOTweenSettings src = DOTweenUtilityWindow.GetDOTweenSettings();
            src.createASMDEF = false;
            EditorUtility.SetDirty(src);
            RemoveASMDEF(ASMDEFType.Modules);
            if (hasProASMDEF) RemoveASMDEF(ASMDEFType.DOTweenPro);
            if (hasProEditorASMDEF) RemoveASMDEF(ASMDEFType.DOTweenProEditor);
            if (hasDOTweenTimelineASMDEF) RemoveASMDEF(ASMDEFType.DOTweenTimeline);
            if (hasDOTweenTimelineEditorASMDEF) RemoveASMDEF(ASMDEFType.DOTweenTimelineEditor);
            EditorUtils.RemoveGlobalDefine(DOTweenDefines.GlobalDefine_DOTween_ASDMEF);
        }

        #endregion

        #region Methods

        static void ValidateProASMDEFReferences(DOTweenSettings src, ASMDEFType asmdefType, string asmdefFilepath)
        {
            bool hasTextMeshProRef = false;
            using (StreamReader sr = new StreamReader(asmdefFilepath)) {
                string s;
                while ((s = sr.ReadLine()) != null) {
                    if (!s.Contains(_RefTextMeshPro)) continue;
                    hasTextMeshProRef = true;
                    break;
                }
            }
            bool recreate = hasTextMeshProRef != src.modules.textMeshProEnabled;
            if (recreate) CreateASMDEF(asmdefType, true);
        }
        static void ValidateDOTweenTimelineASMDEFReferences(DOTweenSettings src, ASMDEFType asmdefType, string asmdefFilepath)
        {
            bool hasTextMeshProRef = false;
            using (StreamReader sr = new StreamReader(asmdefFilepath)) {
                string s;
                while ((s = sr.ReadLine()) != null) {
                    if (!s.Contains(_RefTextMeshPro)) continue;
                    hasTextMeshProRef = true;
                    break;
                }
            }
            bool recreate = hasTextMeshProRef != src.modules.textMeshProEnabled;
            if (recreate) CreateASMDEF(asmdefType, true);
        }

        static void LogASMDEFChange(ASMDEFType asmdefType, ChangeType changeType)
        {
            string asmdefTypeStr = "";
            switch (asmdefType) {
            case ASMDEFType.Modules:
                asmdefTypeStr = "DOTween/Modules/" + _ModulesASMDEFFile;
                break;
            case ASMDEFType.DOTweenPro:
                asmdefTypeStr = "DOTweenPro/" + _ProASMDEFFile;
                break;
            case ASMDEFType.DOTweenProEditor:
                asmdefTypeStr = "DOTweenPro/Editor/" + _ProEditorASMDEFFile;
                break;
            case ASMDEFType.DOTweenTimeline:
                asmdefTypeStr = "DOTweenTimeline/Scripts/" + _DOTweenTimelineASMDEFFile;
                break;
            case ASMDEFType.DOTweenTimelineEditor:
                asmdefTypeStr = "DOTweenTimeline/Scripts/Editor/" + _DOTweenTimelineEditorASMDEFFile;
                break;
            }
            Debug.Log(string.Format(
                "<b>DOTween ASMDEF file <color=#{0}>{1}</color></b> ► {2}",
                changeType == ChangeType.Deleted ? "ff0000" : changeType == ChangeType.Created ? "00ff00" : "ff6600",
                changeType == ChangeType.Deleted ? "removed" : changeType == ChangeType.Created ? "created" : "changed",
                asmdefTypeStr
            ));
        }

        static void CreateASMDEF(ASMDEFType type, bool forceOverwrite = false)
        {
            Refresh();
            bool alreadyPresent = false;
            string asmdefId = null;
            string asmdefFile = null;
            string asmdefDir = null; // with final OS slash
            switch (type) {
            case ASMDEFType.Modules:
                alreadyPresent = hasModulesASMDEF;
                asmdefId = _ModulesId;
                asmdefFile = _ModulesASMDEFFile;
                asmdefDir = EditorUtils.dotweenModulesDir;
                break;
            case ASMDEFType.DOTweenPro:
                alreadyPresent = hasProASMDEF;
                asmdefId = _ProId;
                asmdefFile = _ProASMDEFFile;
                asmdefDir = EditorUtils.dotweenProDir;
                break;
            case ASMDEFType.DOTweenProEditor:
                alreadyPresent = hasProEditorASMDEF;
                asmdefId = _ProEditorId;
                asmdefFile = _ProEditorASMDEFFile;
                asmdefDir = EditorUtils.dotweenProEditorDir;
                break;
            case ASMDEFType.DOTweenTimeline:
                alreadyPresent = hasDOTweenTimelineASMDEF;
                asmdefId = _DOTweenTimelineId;
                asmdefFile = _DOTweenTimelineASMDEFFile;
                asmdefDir = EditorUtils.dotweenTimelineScriptsDir;
                break;
            case ASMDEFType.DOTweenTimelineEditor:
                alreadyPresent = hasDOTweenTimelineEditorASMDEF;
                asmdefId = _DOTweenTimelineEditorId;
                asmdefFile = _DOTweenTimelineEditorASMDEFFile;
                asmdefDir = EditorUtils.dotweenTimelineEditorScriptsDir;
                break;
            }
            if (alreadyPresent && !forceOverwrite) {
                // EditorUtility.DisplayDialog("Create ASMDEF", asmdefFile + " already exists", "Ok");
                return;
            }
            if (!Directory.Exists(asmdefDir)) {
                EditorUtility.DisplayDialog(
                    "Create ASMDEF",
                    string.Format("Directory not found\n({0})", asmdefDir),
                    "Ok"
                );
                return;
            }

            DOTweenSettings src;
            string asmdefFilePath = asmdefDir + asmdefFile;
            using (StreamWriter sw = File.CreateText(asmdefFilePath)) {
                sw.WriteLine("{");
                switch (type) {
                case ASMDEFType.Modules:
                    sw.WriteLine("\t\"name\": \"{0}\"", asmdefId);
                    break;
                case ASMDEFType.DOTweenPro:
                case ASMDEFType.DOTweenProEditor:
                    sw.WriteLine("\t\"name\": \"{0}\",", asmdefId);
                    sw.WriteLine("\t\"references\": [");
                    src = DOTweenUtilityWindow.GetDOTweenSettings();
                    if (src != null) {
                        if (src.modules.textMeshProEnabled) sw.WriteLine("\t\t\"{0}\",", _RefTextMeshPro);
                    }
                    if (type == ASMDEFType.DOTweenProEditor) {
                        sw.WriteLine("\t\t\"{0}\",", _ModulesId);
                        sw.WriteLine("\t\t\"{0}\"", _ProId);
                        sw.WriteLine("\t],");
                        sw.WriteLine("\t\"includePlatforms\": [");
                        sw.WriteLine("\t\t\"Editor\"");
                        sw.WriteLine("\t],");
                        sw.WriteLine("\t\"autoReferenced\": false");
                    } else {
                        sw.WriteLine("\t\t\"{0}\"", _ModulesId);
                        sw.WriteLine("\t]");
                    }
                    break;
                case ASMDEFType.DOTweenTimeline:
                case ASMDEFType.DOTweenTimelineEditor:
                    sw.WriteLine("\t\"name\": \"{0}\",", asmdefId);
                    sw.WriteLine("\t\"references\": [");
                    src = DOTweenUtilityWindow.GetDOTweenSettings();
                    if (src != null) {
                        if (src.modules.textMeshProEnabled) sw.WriteLine("\t\t\"{0}\",", _RefTextMeshPro);
                    }
                    if (type == ASMDEFType.DOTweenTimelineEditor) {
                        if (EditorUtils.hasPro) sw.WriteLine("\t\t\"{0}\",", _ProId);
                        sw.WriteLine("\t\t\"{0}\",", _ModulesId);
                        sw.WriteLine("\t\t\"{0}\"", _DOTweenTimelineId);
                        sw.WriteLine("\t],");
                        sw.WriteLine("\t\"includePlatforms\": [");
                        sw.WriteLine("\t\t\"Editor\"");
                        sw.WriteLine("\t],");
                        sw.WriteLine("\t\"autoReferenced\": false");
                    } else {
                        if (EditorUtils.hasPro) sw.WriteLine("\t\t\"{0}\",", _ProId);
                        sw.WriteLine("\t\t\"{0}\"", _ModulesId);
                        sw.WriteLine("\t]");
                    }
                    break;
                }
                sw.WriteLine("}");
            }
            string adbFilePath = EditorUtils.FullPathToADBPath(asmdefFilePath);
            AssetDatabase.ImportAsset(adbFilePath, ImportAssetOptions.ForceUpdate);
            Refresh();
            LogASMDEFChange(type, alreadyPresent ? ChangeType.Overwritten : ChangeType.Created);
        }

        static void RemoveASMDEF(ASMDEFType type)
        {
            bool alreadyPresent = false;
            string asmdefFile = null;
            string asmdefDir = null; // with final OS slash
            switch (type) {
            case ASMDEFType.Modules:
                alreadyPresent = hasModulesASMDEF;
                asmdefDir = EditorUtils.dotweenModulesDir;
                asmdefFile = _ModulesASMDEFFile;
                break;
            case ASMDEFType.DOTweenPro:
                alreadyPresent = hasProASMDEF;
                asmdefFile = _ProASMDEFFile;
                asmdefDir = EditorUtils.dotweenProDir;
                break;
            case ASMDEFType.DOTweenProEditor:
                alreadyPresent = hasProEditorASMDEF;
                asmdefFile = _ProEditorASMDEFFile;
                asmdefDir = EditorUtils.dotweenProEditorDir;
                break;
            case ASMDEFType.DOTweenTimeline:
                alreadyPresent = hasDOTweenTimelineASMDEF;
                asmdefFile = _DOTweenTimelineASMDEFFile;
                asmdefDir = EditorUtils.dotweenTimelineScriptsDir;
                break;
            case ASMDEFType.DOTweenTimelineEditor:
                alreadyPresent = hasDOTweenTimelineEditorASMDEF;
                asmdefFile = _DOTweenTimelineEditorASMDEFFile;
                asmdefDir = EditorUtils.dotweenTimelineEditorScriptsDir;
                break;
            }

            Refresh();
            if (!alreadyPresent) {
                // EditorUtility.DisplayDialog("Remove ASMDEF", asmdefFile + " not present", "Ok");
                return;
            }

            string asmdefFilePath = asmdefDir + asmdefFile;
            AssetDatabase.DeleteAsset(EditorUtils.FullPathToADBPath(asmdefFilePath));
            Refresh();
            LogASMDEFChange(type, ChangeType.Deleted);
        }

        #endregion
    }
}