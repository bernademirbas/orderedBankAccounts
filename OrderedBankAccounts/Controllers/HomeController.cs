using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Web.Mvc;
using OrderedBankAccounts.Models;

namespace OrderedBankAccounts.Controllers
{
	public class HomeController : Controller
	{
		public ActionResult Index()
		{
			return View();
		}

		/// <summary>
		/// add file for read and sort inputs
		/// </summary>
		/// <param name="file">posted file</param>
		/// <returns></returns>
		[HttpPost]
		public ActionResult Index(HttpPostedFileBase file)
		{
			if (file != null)
			{
				//check file extension
				var extension = file.ContentType.Split('/')[1].ToUpper();

				if (file.ContentType.Split('/')[1].ToUpper() != "PLAİN")
				{
					ModelState.AddModelError("file", "Please add .txt file");
					return View();
				}

				//read file content and set number of tests 
				string NumberOfTestKey = string.Empty;

				List<string> fileRows = new List<string>();
				using (StreamReader sr = new StreamReader(file.InputStream))
				{
					while (sr.Peek() >= 0)
					{
						if (NumberOfTestKey == string.Empty)
							NumberOfTestKey = sr.ReadLine();
						else
						{
							string str;
							str = sr.ReadLine();
							if (str != string.Empty)
								fileRows.Add(str);
						}
					}
				}

				//file has minimum 3 row : number of tests, number of accounts and list of accounts 
				if (fileRows.Count() > 2)
				{
					//validate number of tests
					int numberOfTests = 0;
					int.TryParse(NumberOfTestKey, out numberOfTests);
					if (numberOfTests > 5 || numberOfTests < 1)
					{
						ModelState.AddModelError("file", "Please fill the .txt file with valid number of tests");
						return View();
					}

					int numberOfAccounts = 0;
					var fileContentList = new List<FileContentModel>();

					for (int i = 0; i < numberOfTests; i++)
					{
						//find number of account processing test
						FileContentModel fileContentModel = new FileContentModel();
						int.TryParse(fileRows.First(), out numberOfAccounts);
						fileContentModel.NumberOfAccounts = numberOfAccounts;

						//find valid accounts
						List<string> accountList = fileRows.Skip(1).Take(numberOfAccounts)
														.Where(ac => ac.Replace(" ", string.Empty).Length == 26 && decimal.TryParse(ac.Replace(" ", string.Empty), out decimal value))
														.Select(a => a).ToList();

						//check account number is valid 
						if (accountList.Count() != numberOfAccounts)
						{
							ModelState.AddModelError("file", "Please fill the .txt file with valid list of account");
							return View();
						}

						//validate model
						if (TryValidateModel(fileContentModel))
						{
							//add file content results to accounts that sorted and specified number of occurency
							fileContentModel.Accounts = accountList.OrderBy(a => a)
										   .GroupBy(ac => ac).Select(r => new KeyValuePair<string, string>(r.Key, r.Count().ToString())).Distinct().ToList();
							fileContentList.Add(fileContentModel);

							//remove added accounts and account number lines from file rows
							fileRows.RemoveRange(0, numberOfAccounts + 1);
						}
						else
						{
							ModelState.AddModelError("file", "Please fill the .txt file valid format of inputs");
							return View();
						}
					}
					return Result(fileContentList);					
				}
			}
			ModelState.AddModelError("file", "Please add file");
			return View();
		}

		/// <summary>
		/// Result for reordered file contents
		/// </summary>
		/// <param name="fileContentModelList"></param>
		/// <returns></returns>
		public ActionResult Result(List<FileContentModel> fileContentModelList)
		{
			return View("Result", fileContentModelList);
		}

	}
}