using Sitecore.Caching;
using Sitecore.Common;
using Sitecore.Configuration;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Data.Managers;
using Sitecore.Globalization;
using Sitecore.Sites;
using Sitecore.Data.LanguageFallback;

namespace Sitecore.Support.Data.LanguageFallback
{
    /// <summary>
    /// LanguageFallbackValuesProvider class
    /// </summary>
    public class LanguageFallbackFieldValuesProvider : Sitecore.Data.LanguageFallback.LanguageFallbackFieldValuesProvider
    {
     // Sitecore.Data.LanguageFallback.LanguageFallbackFieldValuesProvider
     /// <summary>
     /// Determines whether valid is valid to have fallback value.
     /// </summary>
     /// <param name="field">The field to check.</param>
     /// <returns></returns>
        public override bool IsValidForFallback(Field field)
        {
            bool? currentValue = Switcher<bool?, LanguageFallbackFieldSwitcher>.CurrentValue;
            if (currentValue == false)
            {
                return false;
            }
            SiteContext site;
            if (currentValue != true && ((site = Context.Site) == null || !site.SiteInfo.EnableFieldLanguageFallback))
            {
                return false;
            }
            bool flag = true;
            Item item = field.Item;
            IsLanguageFallbackValidCacheKey key = new IsLanguageFallbackValidCacheKey(item.ID.ToString(), field.ID.ToString(), field.Database.Name, field.Language.Name);
            object fallbackIsValidValueFromCache = this.GetFallbackIsValidValueFromCache(field, key);
            if (fallbackIsValidValueFromCache != null)
            {
                flag = ((string)fallbackIsValidValueFromCache == "1");
                return flag;
            }
            Language fallbackLanguage = LanguageFallbackManager.GetFallbackLanguage(item.Language, item.Database, item.ID);
            if (fallbackLanguage == null || string.IsNullOrEmpty(fallbackLanguage.Name))
            {
                flag = false;
            }
            else if (field.Shared)
            {
                flag = false;
            }
            else if (this.ShouldStandardFieldBeSkipped(field))
            {
                flag = false;
            }
            else if (StandardValuesManager.IsStandardValuesHolder(item))
            {
                flag = false;
            }
            else if (field.ID == FieldIDs.EnableLanguageFallback || field.ID == FieldIDs.EnableSharedLanguageFallback)
            {
                flag = false;
            }
            else if (!field.SharedLanguageFallbackEnabled)
            {
                if (Settings.LanguageFallback.AllowVaryFallbackSettingsPerLanguage)
                {
                    Item innerItem;
                    using (new LanguageFallbackItemSwitcher(new bool?(false)))
                    {
                        innerItem = field.InnerItem;
                    }
                    if (innerItem == null || innerItem.Fields[FieldIDs.EnableLanguageFallback].GetValue(false, false) != "1")
                    {
                        flag = false;
                    }
                }
                else
                {
                    Item innerItem2;
                    using (new LanguageFallbackItemSwitcher(new bool?(false)))
                    {
                        innerItem2 = field.InnerItem;
                    }
                    if (innerItem2 == null || innerItem2.Fields[FieldIDs.EnableSharedLanguageFallback].GetValue(true, false) != "1")
                    {
                        flag = false;
                    }
                }
            }
            this.AddFallbackIsValidValueToCache(field, key, flag);
            return flag;
        }
    }
}