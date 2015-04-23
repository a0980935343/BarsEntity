﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using EnvDTE;
using VSLangProj;

namespace Barsix.BarsEntity
{
    public static class ProjectExt
    {
        public static bool HasReference(this Project project, string referenceName)
        {
            var vsproject = project.Object as VSProject;

            foreach (Reference reference in vsproject.References)
            {
                if (reference.Name == referenceName)
                    return true;
            }

            return false;
        }

        public static string RootFolder(this Project project)
        {
            Property prop = null;

            try
            {
                prop = project.Properties.Item("FullPath");
            }
            catch (ArgumentException)
            {
                try
                {
                    prop = project.Properties.Item("ProjectDirectory");
                }
                catch (ArgumentException)
                {
                    prop = project.Properties.Item("ProjectPath");
                }
            }

            if (prop != null)
            {
                string value = prop.Value.ToString();

                if (File.Exists(value))
                {
                    return Path.GetDirectoryName(value);
                }
                else if (Directory.Exists(value))
                {
                    return value;
                }

                throw new ArgumentException("Не найден путь проекта " + project.Name);
            }
            else
            {
                throw new ArgumentException("Не найден путь проекта " + project.Name);
            }
        }
    }
}