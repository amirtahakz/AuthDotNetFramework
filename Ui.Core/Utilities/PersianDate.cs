using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Core.Utilities
{
	public static class PersianDate
	{
		#region Methods

		public static string ConvertToShamsi(this DateTime value)
		{
			PersianCalendar persiancal = new PersianCalendar();
			return persiancal.GetYear(value) + "/" + persiancal.GetMonth(value).ToString("00") + "/" +
				  persiancal.GetDayOfMonth(value).ToString("00");
		}

		#endregion

	}
}
