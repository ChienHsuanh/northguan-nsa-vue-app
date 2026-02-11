using Microsoft.EntityFrameworkCore;
using northguan_nsa_vue_app.Server.Models;

namespace northguan_nsa_vue_app.Server.Extensions
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// 自動為所有實體配置時間戳記屬性
        /// </summary>
        public static void ConfigureTimestamps(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                // 跳過 Identity 內建實體 (除了我們的自定義實體)
                if (entityType.ClrType.Namespace?.StartsWith("Microsoft.AspNetCore.Identity") == true &&
                    entityType.ClrType != typeof(ApplicationUser) &&
                    entityType.ClrType != typeof(ApplicationRole))
                {
                    continue;
                }

                // 配置 CreatedAt 屬性
                var createdAtProperty = entityType.FindProperty("CreatedAt");
                if (createdAtProperty != null && createdAtProperty.ClrType == typeof(DateTime))
                {
                    createdAtProperty.SetDefaultValueSql("GETUTCDATE()");
                }

                // 配置 UpdatedAt 屬性
                var updatedAtProperty = entityType.FindProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.ClrType == typeof(DateTime))
                {
                    updatedAtProperty.SetDefaultValueSql("GETUTCDATE()");
                    updatedAtProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.OnAddOrUpdate;
                }
            }
        }

        /// <summary>
        /// 自動為所有 decimal 屬性配置精度 (針對經緯度等)
        /// </summary>
        public static void ConfigureDecimalPrecision(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(decimal) || property.ClrType == typeof(decimal?))
                    {
                        // 根據屬性名稱設定不同的精度
                        if (property.Name.Equals("Lat", StringComparison.OrdinalIgnoreCase))
                        {
                            property.SetPrecision(10);
                            property.SetScale(8);
                        }
                        else if (property.Name.Equals("Lng", StringComparison.OrdinalIgnoreCase))
                        {
                            property.SetPrecision(11);
                            property.SetScale(8);
                        }
                        else if (property.Name.Contains("Speed", StringComparison.OrdinalIgnoreCase))
                        {
                            property.SetPrecision(5);
                            property.SetScale(2);
                        }
                        else
                        {
                            // 預設精度
                            property.SetPrecision(18);
                            property.SetScale(2);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 自動為所有字串屬性配置長度限制
        /// </summary>
        public static void ConfigureStringLengths(this ModelBuilder modelBuilder)
        {
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    if (property.ClrType == typeof(string))
                    {
                        // 如果沒有明確設定長度，根據屬性名稱設定預設長度
                        if (property.GetMaxLength() == null)
                        {
                            if (property.Name.Contains("Url", StringComparison.OrdinalIgnoreCase))
                            {
                                property.SetMaxLength(500);
                            }
                            else if (property.Name.Equals("Name", StringComparison.OrdinalIgnoreCase) ||
                                     property.Name.Equals("Username", StringComparison.OrdinalIgnoreCase) ||
                                     property.Name.Equals("Email", StringComparison.OrdinalIgnoreCase))
                            {
                                property.SetMaxLength(255);
                            }
                            else if (property.Name.Equals("Phone", StringComparison.OrdinalIgnoreCase))
                            {
                                property.SetMaxLength(20);
                            }
                            else if (property.Name.Contains("Serial", StringComparison.OrdinalIgnoreCase))
                            {
                                property.SetMaxLength(100);
                            }
                            else
                            {
                                // 預設長度
                                property.SetMaxLength(255);
                            }
                        }
                    }
                }
            }
        }
    }
}