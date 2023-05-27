using System.Drawing;

namespace Foxconn.App.Controllers.Variable
{
    public class GlobalVariable
    {
        public const string Data = "Data";
        public const string DirectoryExcel = "Result\\Excel";
        public const string ConnectionString = "mongodb://localhost:27017";
        public const string DatabaseName = "app";
        public const string BasicConfigurationName = "basic_configuration";
        public const string RuntimeConfigurationName = "runtime_configuration";
        public const string StatisticsDataName = "statistics_data";
        public const string StorageDataName = "storage_data";
        public const string ImageName = "image.png";
        public const string FeatureImageName = "fm_image.png";
        public const string TemplateImageName = "tm_image.png";
        public static SizeF PaddingMark = new SizeF((float)1.5, (float)1.5);
    }
}
