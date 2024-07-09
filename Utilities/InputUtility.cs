#region
using Spectre.Console;
#endregion

namespace UnityOps.Utilities
{
    public class InputUtility
    {
        //Takes in a list of type T and iterate over it and puts a choices.... 
        public static string SelectionPrompt<T>(string question, List<T> choiceList, Func<T, string> displaySelector, string moreChoicesText = "[bold blue](Move up and down to reveal more)[/]", int pageSize = 10)
        {
            try
            {
                string[] choices = ListItemsForPrompt(choiceList, displaySelector);

                string selection = DisplayUtility.DisplaySelectionPrompt(question, choices, moreChoicesText, pageSize);

                return selection;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return string.Empty;
            }
        }
        public static List<string> MultiSelectionPrompt<T>(string question, List<T> choiceList, Func<T, string> displaySelector, string moreChoicesText = "[bold blue](Move up and down to reveal more)[/]", int pageSize = 10)
        {
            try
            {
                string[] choices = ListItemsForPrompt(choiceList, displaySelector);

                List<string> selection = DisplayUtility.DisplayMultiSelectionPrompt(question, choices, moreChoicesText, pageSize);

                return selection;
            }
            catch (Exception ex)
            {
                AnsiConsole.WriteException(ex);
                return default;
            }

        }

        public static string DirectoryValidationPrompt(string question, string defaultValue, string defaultStyle = "bold blue")
        {
            string input = AnsiConsole.Prompt(
            new TextPrompt<string>(question)
                .DefaultValueStyle(defaultStyle)
                .DefaultValue(defaultValue)
                .Validate(directory => Directory.Exists(directory) ? ValidationResult.Success() : ValidationResult.Error($"[{Program.ErrorColor}]Directory must exist[/]")));

            return input;
        }
        public static string DirectoryValidationPrompt(string question, string defaultStyle = "bold blue")
        {
            string userInput = AnsiConsole.Prompt(
            new TextPrompt<string>(question)
                .DefaultValueStyle(defaultStyle)
                .Validate(directory => Directory.Exists(directory) ? ValidationResult.Success() : ValidationResult.Error($"[{Program.ErrorColor}]Directory must exist[/]")));

            return userInput;
        }

        private static string[] ListItemsForPrompt<T>(List<T> choiceList, Func<T, string> displaySelector)
        {
            return choiceList.Select(item => displaySelector(item)).ToArray();
        }

        public static string MainMenuSelectionPrompt()
        {
            return AnsiConsole.Prompt(
            new SelectionPrompt<string>().AddChoices([
                "Config", "Find Unity Editors and Projects", "Open Project", "Manage Applications"
            ]
            ));

        }
    }
}