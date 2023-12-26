namespace Web.Models.View.User;

public class CreateLinkView
{
    // [RegularExpression("/((([A-Za-z]{3,9}:(?:\\/\\/)?)(?:[-;:&=\\+\\$,\\w]+@)?[A-Za-z0-9.-]+|(?:www.|[-;:&=\\+\\$,\\w]+@)[A-Za-z0-9.-]+)((?:\\/[\\+~%\\/.\\w-_]*)?\\??(?:[-\\+=&;%@.\\w_]*)#?(?:[\\w]*))?)/")]
    public string NewLinkURL { get; set; }
    public string NewLinkName { get; set; }
    public string NewLinkCode { get; set; }
}

