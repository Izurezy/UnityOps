using Spectre.Console;

namespace UnityOps.Utilities
{
    public class InputUtility
    {
        //Takes in a list of type T and iterate over it and puts a choices.... 
        public static string ListItemsForSelectionPrompt<T>(string question, List<T> choiceList, Func<T, string> displaySelector)
        {
            try
            {
                var choiceListNew = new List<string>();
                foreach (var item in choiceList)
                    choiceListNew.Add(displaySelector(item));

                string[] choices = choiceListNew.ToArray();

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

        public static string MainMenuSelectionPrompt()
        {
            return AnsiConsole.Prompt(
                new SelectionPrompt<string>().
                AddChoices([
                    "Config", "Open Project", "Add Application to Open", "Add or Remove Project", "Add or Remove Editor"
                ]
            ));

        }
    }
}