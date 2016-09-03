using System.Collections.Generic;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.ExperienceEditor.Utils;
using Sitecore.Pipelines.GetLayoutSourceFields;

namespace Sitecore.Boost.SetLayoutRenderings
{
    public class LightSetLayoutRenderings
    {
        public virtual void Process(GetLayoutSourceFieldsArgs args)
        {
            // NM: Believe this doesn't need to happen in normal mode.. ? - Not sure if this should clear ?
            if (Context.PageMode.IsNormal)
            {
                return;
            }

            if (!WebUtility.IsEditAllVersionsTicked())
            {
                return;
            }

            ClearFields(args.FieldValuesSource, FieldIDs.FinalLayoutField);
        }

        protected virtual void ClearFields(List<Field> fields, ID fieldIdToExclude)
        {
            fields.RemoveAll(s => s.ID == fieldIdToExclude);
        }
    }
}