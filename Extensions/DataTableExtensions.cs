using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PageObjectWrapper.AutomationUI.Extensions
{
    public static class DataTableExtensions
    {
        public static DataRow FindByText(this DataTable table, string textToFind, bool indexOf = false)
        {
            if (table == null || table.Rows == null || table.Rows.Count <= 0)
                return null;

            foreach (DataRow row in table.Rows)
            {
                var data = row.ItemArray;
                foreach (var item in data)
                {
                    if (item != null && !string.IsNullOrEmpty(item.ToString()))
                    {
                        if (indexOf && item.ToString().IndexOf(textToFind) >= 0)
                            return row;
                        else if (item.ToString().Equals(textToFind))
                            return row;
                    }
                }
            }

            return null;
        }

        public static bool ContainsText(this DataTable table, string textToFind, bool indexOf = false)
        {
            return table.FindByText(textToFind, indexOf) != null;
        }
    }
}
