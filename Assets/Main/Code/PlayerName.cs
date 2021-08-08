[System.Serializable]

public class PlayerName : ISavable
{
    private const string DEFAULT_NAME = "Player123567";
    public const int MAX_LETTER_COUNT = 16;
    public string name;
    //[SerializeField] private InputField nameInputField;

    public PlayerName()
    {
        //Default values:
        name = DEFAULT_NAME;
    }

    public static string LegaliseName(string name)
    {
        int letterCount = name.Length;
        if (letterCount < 1)
        {
            name = DEFAULT_NAME;
        }
        else if(letterCount > MAX_LETTER_COUNT)
        {
            name = name.Remove(MAX_LETTER_COUNT);
        }
        return name;
    }
    /* public void OnInputFieldUpdated()
     {
         SetName(nameInputField.text);
     }*/

    /* private void SetName(string newName)
     {
         if(newName != string.Empty)
         {
             name = newName;
         }
         else
         {
             name = fallbackName;
         }
     }*/
}

