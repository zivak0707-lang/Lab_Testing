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

            case QuestionType.Categories:
                foreach (QuestionItem item in q.Items)
                {
                    DropZone row = view.GetZone("cat:" + q.Categories.IndexOf(item.Category));
                    view.PlaceCard(Card(view, item.Text), row, DragDropEffects.Move);
                }

                break;

            case QuestionType.Gaps:
                List<string> answers = q.GetGapAnswers();

                for (int i = 0; i < answers.Count; i++)
                {
                    // Слово з палітри копіюється (Copy), як під час перетягування
                    CardLabel template = view.Cards.First(c => c.IsTemplate && c.Value == answers[i]);
                    view.PlaceCard(template, view.GetZone("gap:" + i), DragDropEffects.Copy);
                }

                break;

            case QuestionType.Order:
                for (int i = 0; i < q.Steps.Count; i++)
                {
                    view.PlaceCard(Card(view, q.Steps[i]), view.GetZone("slot:" + i), DragDropEffects.Move);
                }

                break;
        }
    }

    // Відповідь з типовими помилками (для знімків перевірки і самоперевірки балів)
    public static void FillWithMistakes(QuestionView view)
    {
        Question q = view.Question;

        switch (q.Type)
        {
            case QuestionType.Basket:
                // Усі зайві прибрано, але й одну потрібну картку теж прибрано
                foreach (QuestionItem item in q.Items.Where(i => !i.Keep).Append(q.Items.First(i => i.Keep)))
                {
                    view.PlaceCard(Card(view, item.Text), view.GetZone("basket"), DragDropEffects.Move);
                }

                break;

            case QuestionType.Table:
                // Відповіді в перших двох рядках переплутано
                for (int i = 0; i < q.Rows.Count; i++)
                {
                    int source = i == 0 ? 1 : i == 1 ? 0 : i;
                    view.PlaceCard(Card(view, q.Rows[source].Answer), view.GetZone("row:" + i), DragDropEffects.Move);
                }

                break;

            case QuestionType.Categories:
                // Перший елемент покладено не в ту категорію
                for (int i = 0; i < q.Items.Count; i++)
                {
                    int category = q.Categories.IndexOf(q.Items[i].Category);

                    if (i == 0)
                    {
                        category = (category + 1) % q.Categories.Count;
                    }

                    view.PlaceCard(Card(view, q.Items[i].Text), view.GetZone("cat:" + category), DragDropEffects.Move);
                }

                break;

            case QuestionType.Gaps:
                // Перший пропуск — зайве слово, другий порожній, решта правильні
                List<string> answers = q.GetGapAnswers();

                for (int i = 0; i < answers.Count; i++)
                {
                    if (i == 1)
                    {
                        continue;
                    }

                    string word = i == 0 ? q.ExtraWords[0] : answers[i];
                    CardLabel template = view.Cards.First(c => c.IsTemplate && c.Value == word);
                    view.PlaceCard(template, view.GetZone("gap:" + i), DragDropEffects.Copy);
                }

                break;

            case QuestionType.Order:
                // Два сусідні кроки ближче до кінця переставлено
                int n = q.Steps.Count;

                for (int i = 0; i < n; i++)
                {
                    int source = i == n - 3 ? n - 2 : i == n - 2 ? n - 3 : i;
                    view.PlaceCard(Card(view, q.Steps[source]), view.GetZone("slot:" + i), DragDropEffects.Move);
                }

                break;
        }
    }

    // Скільки балів має дати FillWithMistakes (рахуємо незалежно від програми)
    public static double ExpectedMistakesScore(Question q)
    {
        double score = q.Type switch
        {
            QuestionType.Basket => 20.0 * (q.Items.Count(i => !i.Keep) - 1) / q.Items.Count(i => !i.Keep),
            QuestionType.Table => 20.0 * (q.Rows.Count - 2) / q.Rows.Count,
            QuestionType.Categories => 20.0 * (q.Items.Count - 1) / q.Items.Count,
            QuestionType.Gaps => 20.0 * (q.GetGapAnswers().Count - 2) / q.GetGapAnswers().Count,
            _ => 20.0 * (q.Steps.Count - 2) / q.Steps.Count
        };

        return Math.Round(score, 1);
    }

    private static CardLabel Card(QuestionView view, string value)
    {
        return view.Cards.First(c => c.Value == value && !c.IsTemplate);
    }
}
