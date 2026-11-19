namespace Lab_Testing;

// Тимчасова заглушка для типів питань, які ще не реалізовано (буде на етапі 3)
public class PlaceholderView : QuestionView
{
    public PlaceholderView(Question question, Random random) : base(question, random)
    {
        Controls.Add(new Label
        {
            Text = $"Тип завдання «{question.Type.ToUkrainian()}» з'явиться на етапі 3.",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Theme.MutedText
        });
    }

    public override bool HasAnswer => false;
}
