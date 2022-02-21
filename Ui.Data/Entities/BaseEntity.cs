using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ui.Data.Entities
{
	public class BaseEntity
	{
		#region Properties

		[Key]
		public long Id { get; protected set; }

		[Required(ErrorMessage = "{0} is required.")]
		public DateTime DateCreated { get; set; }

		[Required(ErrorMessage = "{0} is required.")]
		public bool IsRemove { get; set; }

		#endregion
	}
}
