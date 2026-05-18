using DevTrack.Domain.Entities;
using DevTrack.Domain.Enums;
using iText.IO.Font.Constants;
using iText.Kernel.Colors;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace DevTrack.API.Services;

public class SprintReportService
{
    private static readonly Color PrimaryColor = new DeviceRgb(26, 86, 219);
    private static readonly Color HeaderBg = new DeviceRgb(239, 246, 255);
    private static readonly Color DoneBg = new DeviceRgb(209, 250, 229);
    private static readonly Color CriticalColor = new DeviceRgb(220, 38, 38);
    private static readonly Color HighColor = new DeviceRgb(234, 88, 12);

    private static PdfFont BoldFont => PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);
    private static PdfFont RegularFont => PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
    private static PdfFont ItalicFont => PdfFontFactory.CreateFont(StandardFonts.HELVETICA_OBLIQUE);

    public byte[] GenerateSprintReport(Project project, IEnumerable<Ticket> tickets)
    {
        var ticketList = tickets.ToList();
        using var stream = new MemoryStream();
        using var writer = new PdfWriter(stream);
        using var pdf = new PdfDocument(writer);
        using var doc = new Document(pdf);

        doc.SetMargins(40, 40, 40, 40);

        AddHeader(doc, project);
        AddSummaryStats(doc, ticketList);
        AddPriorityBreakdown(doc, ticketList);
        AddTicketTable(doc, ticketList);
        AddFooter(doc);

        doc.Close();
        return stream.ToArray();
    }

    private static void AddHeader(Document doc, Project project)
    {
        doc.Add(new Paragraph($"Sprint Report — {project.Name}")
            .SetFont(BoldFont)
            .SetFontSize(22)
            .SetFontColor(PrimaryColor)
            .SetMarginBottom(4));

        doc.Add(new Paragraph($"Generated on {DateTime.UtcNow:MMMM dd, yyyy} UTC")
            .SetFont(RegularFont)
            .SetFontSize(10)
            .SetFontColor(ColorConstants.GRAY)
            .SetMarginBottom(4));

        if (!string.IsNullOrWhiteSpace(project.Description))
        {
            doc.Add(new Paragraph(project.Description)
                .SetFont(ItalicFont)
                .SetFontSize(10)
                .SetMarginBottom(4));
        }

        if (project.Deadline.HasValue)
        {
            doc.Add(new Paragraph($"Deadline: {project.Deadline.Value:MMMM dd, yyyy}")
                .SetFont(RegularFont)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.DARK_GRAY)
                .SetMarginBottom(16));
        }

        doc.Add(new LineSeparator(new SolidLine()).SetMarginBottom(16));
    }

    private static void AddSummaryStats(Document doc, List<Ticket> tickets)
    {
        int total = tickets.Count;
        int done = tickets.Count(t => t.Status == TicketStatus.Done);
        int inProgress = tickets.Count(t => t.Status == TicketStatus.InProgress);
        int backlog = tickets.Count(t => t.Status == TicketStatus.Backlog);
        int review = tickets.Count(t => t.Status == TicketStatus.Review);
        int critical = tickets.Count(t => t.Priority == TicketPriority.Critical);
        float completion = total > 0 ? (float)done / total * 100 : 0;

        doc.Add(new Paragraph("Summary")
            .SetFont(BoldFont)
            .SetFontSize(14)
            .SetFontColor(PrimaryColor)
            .SetMarginBottom(8));

        var table = new Table(UnitValue.CreatePercentArray([1f, 1f, 1f, 1f, 1f, 1f]))
            .UseAllAvailableWidth()
            .SetMarginBottom(16);

        AddStatCell(table, "Total", total.ToString());
        AddStatCell(table, "Done", done.ToString(), DoneBg);
        AddStatCell(table, "In Progress", inProgress.ToString());
        AddStatCell(table, "Review", review.ToString());
        AddStatCell(table, "Backlog", backlog.ToString());
        AddStatCell(table, "Completion", $"{completion:F0}%", completion >= 80 ? DoneBg : ColorConstants.WHITE);

        doc.Add(table);

        if (critical > 0)
        {
            doc.Add(new Paragraph($"! {critical} critical priority ticket(s) open")
                .SetFont(BoldFont)
                .SetFontSize(10)
                .SetFontColor(CriticalColor)
                .SetMarginBottom(16));
        }
    }

    private static void AddPriorityBreakdown(Document doc, List<Ticket> tickets)
    {
        doc.Add(new Paragraph("Priority Breakdown")
            .SetFont(BoldFont)
            .SetFontSize(14)
            .SetFontColor(PrimaryColor)
            .SetMarginBottom(8));

        var table = new Table(UnitValue.CreatePercentArray([1f, 1f, 1f, 1f]))
            .UseAllAvailableWidth()
            .SetMarginBottom(16);

        foreach (var priority in Enum.GetValues<TicketPriority>().Reverse())
        {
            int count = tickets.Count(t => t.Priority == priority);
            int donePriority = tickets.Count(t => t.Priority == priority && t.Status == TicketStatus.Done);
            var labelColor = priority switch
            {
                TicketPriority.Critical => CriticalColor,
                TicketPriority.High => HighColor,
                _ => PrimaryColor
            };

            var cell = new Cell().SetBackgroundColor(HeaderBg).SetPadding(8);
            cell.Add(new Paragraph(priority.ToString())
                .SetFont(BoldFont)
                .SetFontColor(labelColor)
                .SetFontSize(11));
            cell.Add(new Paragraph($"{count} tickets ({donePriority} done)")
                .SetFont(RegularFont)
                .SetFontSize(9)
                .SetFontColor(ColorConstants.DARK_GRAY));
            table.AddCell(cell);
        }

        doc.Add(table);
    }

    private static void AddTicketTable(Document doc, List<Ticket> tickets)
    {
        doc.Add(new Paragraph("Ticket Details")
            .SetFont(BoldFont)
            .SetFontSize(14)
            .SetFontColor(PrimaryColor)
            .SetMarginBottom(8));

        var table = new Table(UnitValue.CreatePercentArray([0.35f, 0.15f, 0.15f, 0.2f, 0.1f, 0.05f]))
            .UseAllAvailableWidth()
            .SetMarginBottom(16);

        foreach (var header in new[] { "Title", "Status", "Priority", "Assignee", "Due Date", "#" })
        {
            table.AddHeaderCell(new Cell()
                .SetBackgroundColor(PrimaryColor)
                .SetFontColor(ColorConstants.WHITE)
                .SetFont(BoldFont)
                .SetFontSize(9)
                .SetPadding(6)
                .Add(new Paragraph(header).SetFont(BoldFont)));
        }

        foreach (var ticket in tickets.OrderBy(t => t.Status).ThenByDescending(t => t.Priority))
        {
            var rowBg = ticket.Status == TicketStatus.Done ? DoneBg : ColorConstants.WHITE;

            table.AddCell(MakeCell(Truncate(ticket.Title, 50), rowBg));
            table.AddCell(MakeCell(ticket.Status.ToString(), rowBg));

            var priorityColor = ticket.Priority switch
            {
                TicketPriority.Critical => CriticalColor,
                TicketPriority.High => HighColor,
                _ => ColorConstants.BLACK
            };
            table.AddCell(new Cell().SetBackgroundColor(rowBg).SetPadding(5)
                .Add(new Paragraph(ticket.Priority.ToString())
                    .SetFont(RegularFont)
                    .SetFontSize(9)
                    .SetFontColor(priorityColor)));

            var assignee = ticket.Assignee is null
                ? "Unassigned"
                : ticket.Assignee.FirstName + " " + ticket.Assignee.LastName;
            table.AddCell(MakeCell(assignee, rowBg));
            table.AddCell(MakeCell(ticket.DueDate?.ToString("MM/dd/yy") ?? "-", rowBg));
            table.AddCell(new Cell().SetBackgroundColor(rowBg).SetPadding(5)
                .Add(new Paragraph(ticket.Comments.Count.ToString())
                    .SetFont(RegularFont)
                    .SetFontSize(9)
                    .SetTextAlignment(TextAlignment.CENTER)));
        }

        doc.Add(table);
    }

    private static void AddFooter(Document doc)
    {
        doc.Add(new LineSeparator(new SolidLine()).SetMarginTop(8).SetMarginBottom(8));
        doc.Add(new Paragraph("DevTrack — Project Management Platform")
            .SetFont(RegularFont)
            .SetFontSize(8)
            .SetFontColor(ColorConstants.GRAY)
            .SetTextAlignment(TextAlignment.CENTER));
    }

    private static Cell MakeCell(string text, Color bg) =>
        new Cell().SetBackgroundColor(bg).SetPadding(5)
            .Add(new Paragraph(text).SetFont(RegularFont).SetFontSize(9));

    private static void AddStatCell(Table table, string label, string value, Color? bg = null)
    {
        var cell = new Cell()
            .SetBackgroundColor(bg ?? HeaderBg)
            .SetPadding(10)
            .SetTextAlignment(TextAlignment.CENTER);
        cell.Add(new Paragraph(value).SetFont(BoldFont).SetFontSize(20).SetMarginBottom(2));
        cell.Add(new Paragraph(label).SetFont(RegularFont).SetFontSize(9).SetFontColor(ColorConstants.DARK_GRAY));
        table.AddCell(cell);
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max] + "...";
}
