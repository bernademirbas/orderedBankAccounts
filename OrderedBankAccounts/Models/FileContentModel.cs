using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace OrderedBankAccounts.Models
{
	public class FileContentModel
	{

		[Range(typeof(int), "1", "100000")]
		public int NumberOfAccounts { get; set; }
		
		public List<KeyValuePair<string, string>> Accounts { get; set; }
	}	
}