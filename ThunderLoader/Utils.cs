using System;
using System.IO;
using System.Reflection;
using System.Text;

namespace Mythic.ThunderLoader
{
    public class Utils
    {
        public static string TypeLoadExceptionToString(ReflectionTypeLoadException ex)
        {
            StringBuilder sb = new StringBuilder();
            foreach (Exception exSub in ex.LoaderExceptions)
            {
                sb.AppendLine(exSub.Message);
                if (exSub is FileNotFoundException exFileNotFound)
                {
                    if (!string.IsNullOrEmpty(exFileNotFound.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exFileNotFound.FusionLog);
                    }
                }
                else if (exSub is FileLoadException exLoad)
                {
                    if (!string.IsNullOrEmpty(exLoad.FusionLog))
                    {
                        sb.AppendLine("Fusion Log:");
                        sb.AppendLine(exLoad.FusionLog);
                    }
                }

                sb.AppendLine();
            }

            return sb.ToString();
        }
    }
}
