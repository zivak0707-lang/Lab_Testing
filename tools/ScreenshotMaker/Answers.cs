using Lab_Testing;

namespace ScreenshotMaker;

// Розкладає відповіді в питанні тим самим методом PlaceCard, який викликає обробник DragDrop
internal static class Answers
{
    // Усі відповіді правильні
    public static void FillCorrect(QuestionView view)
    {
        Question q = view.Question;

        switch (q.Type)
        {
            case QuestionType.Basket:
                foreach (QuestionItem item in q.Items.Where(i => !i.Keep))
                {
                    view.PlaceCard(Card(view, item.Text), view.GetZone("basket"), DragDropEffects.Move);
                }

                break;

            case QuestionType.Table:
                for (int i = 0; i < q.Rows.Count; i++)
                {
                    view.PlaceCard(Card(view, q.Rows[i].Answer), view.GetZone("row:" + i), DragDropEffects.Move);
                }

                break;
        }
    }

    private static CardLabel Card(QuestionView view, string value)
    {
        return view.Cards.First(c => c.Value == value && !c.IsTemplate);
    }
}
