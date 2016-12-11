using System;
using System.Collections.Generic;
using System.Linq;
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
    using CodeGeneration.CSharp;

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideAutoLoad(UIContextGuids80.SolutionExists)]
    [InstalledProductRegistration("#110", "#112", Version, IconResourceID = 400)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [Guid(GuidList.guidAddEntityPkgString)]
    public sealed class BarsEntityPackage : ExtensionPointPackage
    {
        private static DTE2 _dte;
        public const string Version = "1.7";

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

                CommandID addTMenuCommandID = new CommandID(GuidList.guidAddQuartzTaskCmdSet, (int)PkgCmdIDList.cmdidAddQuartzTask);
                MenuCommand addTMenuItem = new MenuCommand(QuartzTaskMenuItemCallback, addTMenuCommandID);
                mcs.AddCommand(addTMenuItem);
            }
        }

        private void MenuItemCallback(object sender, EventArgs e)
        {
            Project project = GetSelectedProject();
            
            EntityOptionsWindow dialog = new EntityOptionsWindow();
            dialog.SetProject(project);
            dialog.Show();
        }

        private void MigrationMenuItemCallback(object sender, EventArgs e)
        {
            Project project = GetSelectedProject();

            GenerationManager manager = new GenerationManager(project, false);
            manager.AddGenerator(new MigrationGenerator());

            EntityOptions Options = new EntityOptions (project.GetProjectProfile());
            Options.MigrationVersion = Interaction.InputBox("Укажите версию. При увеличении от последней версии оставьте пустым", 
                                                            "Миграция", DateTime.Now.ToString("yyyy_MM_dd_00"));

            if (string.IsNullOrEmpty(Options.MigrationVersion))
            {
                Options.FromLastMigration = true;
                Options.MigrationVersion = DateTime.Now.ToString("yyyy_MM_dd_00");
            }

            if (!Options.FromLastMigration && File.Exists( Path.Combine( project.RootFolder(), "Migrations\\{0}\\UpdateSchema.cs".R(Options.MigrationVersion))))
            {
                MessageBox.Show("Миграция с этим номером версии уже существует! Измените версию", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                manager.UsedGenerators = new List<Type> { typeof(MigrationGenerator) };
                manager.AddToProject(Options);
                manager.InsertFragments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var file = manager.Files.SelectMany(x => x.Value).First();
            
            Window window = _dte.ItemOperations.OpenFile(Path.Combine(project.RootFolder(), file.Path, file.Name));
            return;
        }

        private void QuartzTaskMenuItemCallback(object sender, EventArgs e)
        {
            Project project = GetSelectedProject();

            GenerationManager manager = new GenerationManager(project, false);
            manager.AddGenerator(new QuartzTaskGenerator());

            EntityOptions Options = new EntityOptions(project.GetProjectProfile());
            Options.ClassName = Interaction.InputBox("Укажите название задача", "Quartz-задача", "NewTask");

            if (Options.ClassName == "")
                return;

            if (File.Exists(Path.Combine(project.RootFolder(), "Tasks\\{0}.cs".R(Options.MigrationVersion))))
            {
                MessageBox.Show("Такая задача уже существует!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (!project.HasReference("Bars.B4.Modules.Quartz"))
            {
                if (MessageBox.Show("В проекте {0} нет ссылки на Bars.B4.Modules.Quartz!\nПродолжить?".R(project.Name), "Помни!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.No)
                    return;
            };

            try
            {
                manager.UsedGenerators = new List<Type> { typeof(QuartzTaskGenerator) };
                manager.AddToProject(Options);
                manager.InsertFragments();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var file = manager.Files.SelectMany(x => x.Value).First();

            if (manager.Fragments.Any())
            {
                DontForgetThis form = new DontForgetThis();
                form.richTextBox1.Text = string.Join(Environment.NewLine, manager.Fragments.ToList());
                form.ShowDialog();
            }

            Window window = _dte.ItemOperations.OpenFile(Path.Combine(project.RootFolder(), file.Path, file.Name));
            return;
        }

        private static Project GetSelectedProject()
        {
            var items = (Array)_dte.ToolWindows.SolutionExplorer.SelectedItems;
            UIHierarchyItem item = null;

            foreach (UIHierarchyItem selItem in items)
            {
                item = selItem;
                break;
            }
            
            if (item == null)
                return null;

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
            return project;
        }
    }
}
