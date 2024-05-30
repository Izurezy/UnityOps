using System.Diagnostics.Contracts;

using System.Linq;
using Spectre.Console;
using UnityOps.Structs;

namespace UnityOps.Utilities
{
    public class InputUtility
    {

        public static string SelectionPrompt<T>(string question, List<T> choicesList, Func<T, string> displaySelector)
        {
            try
            {
                var choicesListNew = new List<string>();
                foreach (var item in choicesList)
                    choicesListNew.Add(displaySelector(item));

                string[] choices = choicesListNew.ToArray();

                string selection = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title($"{question}")
                        .PageSize(10)
                        .MoreChoicesText("[blue](Move up and down to reveal more)[/]")
                        .AddChoices(choices));

                return selection;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return string.Empty;
            }
        }
    }
}