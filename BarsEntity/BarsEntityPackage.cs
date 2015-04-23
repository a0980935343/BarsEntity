﻿using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

using EnvDTE;
using EnvDTE80;
using VSLangProj;
using Microsoft.VisualBasic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Barsix.BarsEntity
{
    using BarsGenerators;
    using BarsOptions;
    using CodeGeneration;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidAddEntityPkgString)]
    public sealed class BarsEntityPackage : ExtensionPointPackage
    {
        private static DTE2 _dte;
        public const string Version = "1.9";

        protected override void Initialize()
        {
            _dte = GetService(typeof(DTE)) as DTE2;
            base.Initialize();

            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                CommandID addMenuCommandID = new CommandID(GuidList.guidAddEntityCmdSet, (int)PkgCmdIDList.cmdidAddEntity);
                MenuCommand addMenuItem = new MenuCommand(MenuItemCallback, addMenuCommandID);
                mcs.AddCommand(addMenuItem);

                CommandID addMMenuCommandID = new CommandID(GuidList.guidAddMigrationCmdSet, (int)PkgCmdIDList.cmdidAddMigration);
                MenuCommand addMMenuItem = new MenuCommand(MigrationMenuItemCallback, addMMenuCommandID);
                mcs.AddCommand(addMMenuItem);
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            UIHierarchyItem item = GetSelectedItem();

            if (item == null)
                return;

            ProjectItem projectItem = item.Object as ProjectItem;
            Project project = item.Object as Project;

            if (project == null)
            {
                project = projectItem.ContainingProject;
            }
            if (project == null)
            {
                throw new ArgumentException("Project not found!");
            }

            EntityOptionsDialog dialog = new EntityOptionsDialog();
            dialog.Project = project;
            dialog.Show();
        }

        private void MigrationMenuItemCallback(object sender, EventArgs e)
        {
            UIHierarchyItem item = GetSelectedItem();

            if (item == null)
                return;

            ProjectItem projectItem = item.Object as ProjectItem;
            Project project = item.Object as Project;

            if (project == null)
            {
                project = projectItem.ContainingProject;
            }
            if (project == null)
            {
                throw new ArgumentException("Project not found!");
            }

            EntityOptions Options = new EntityOptions ();
            Options.MigrationVersion = Interaction.InputBox("Укажите версию", "Миграция", DateTime.Now.ToString("yyyy_MM_dd_00"));

            if (Options.MigrationVersion == "")
                return;

            if (File.Exists( Path.Combine( project.RootFolder(), "Migrations\\{0}\\UpdateSchema.cs".F(Options.MigrationVersion))))
            {
                MessageBox.Show("Миграция с этим номером версии уже существует! Измените версию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var gen = new MigrationGenerator();
            try
            {
                gen.Generate(project, Options, new GeneratedFragments());
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Window window = _dte.ItemOperations.OpenFile(Path.Combine(project.RootFolder(), Options.ResultFile));
            return;
        }

        private static UIHierarchyItem GetSelectedItem()
        {
            var items = (Array)_dte.ToolWindows.SolutionExplorer.SelectedItems;

            foreach (UIHierarchyItem selItem in items)
            {
                return selItem;
            }

            return null;
        }
    }
}
