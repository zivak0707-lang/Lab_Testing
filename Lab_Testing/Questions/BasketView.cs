namespace Lab_Testing;

// Тип 1 «Кошик»: на слайді лежать картки, зайві треба перетягнути в кошик
public class BasketView : QuestionView
{
    private readonly DropZone slide;    // слайд із картками
    private readonly DropZone basket;   // кошик для зайвих карток

    public BasketView(Question question, Random random) : base(question, random)
    {
        slide = CreateZone("slide");
        slide.NormalBackColor = Color.White;
        slide.Padding = new Padding(S(14));

        basket = CreateZone("basket", 0, "Перетягніть сюди зайві картки");
        basket.IconGlyph = "";   // значок кошика зі шрифту Segoe MDL2 Assets
        basket.Padding = new Padding(S(10));

        HomeZone = slide;

        // Спочатку всі картки (потрібні й зайві) лежать на слайді в перемішаному порядку
        foreach (QuestionItem item in Shuffle(question.Items))
        {
            slide.Controls.Add(CreateCard(item.Text));
        }

        Controls.Add(CreateColumns(62, CreateSection("Слайд", slide), CreateSection("Кошик", basket)));
    }

    // Відповідь почата, якщо в кошику є хоча б одна картка
    public override bool HasAnswer => basket.Cards.Count > 0;
}
