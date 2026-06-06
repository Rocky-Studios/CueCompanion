using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CueCompanion;

public static class PDFExport
{
    public static byte[] GeneratePDF(ShowBundle bundle)
    {
        Show show = bundle.Show;
        var  cues = bundle.Cues;
        cues.Sort((c1, c2) => c1.Position.CompareTo(c2.Position));
        CueTask[] tasks = bundle.Tasks;
        Role[]    roles = ShowManager.GetRoles();
        return Document.Create(container =>
                               {
                                   container.Page(page =>
                                                  {
                                                      page.Size(PageSizes.A4);
                                                      page.Margin(2, Unit.Centimetre);
                                                      page.PageColor(Colors.White);
                                                      page.DefaultTextStyle(x => x.FontSize(20));

                                                      page.Header()
                                                          .Text("CTHS Media Centre")
                                                          .AlignCenter()
                                                          .SemiBold().FontSize(16).FontColor(Colors.Black);

                                                      page.Content()
                                                          .PaddingVertical(1, Unit.Centimetre)
                                                          .Column(x =>
                                                                  {
                                                                      x.Spacing(20);

                                                                      x.Item()
                                                                       .Text("CTHS Media Centre")
                                                                       .AlignCenter()
                                                                       .Bold().FontSize(36).FontColor(Colors.Red.Medium);

                                                                      x.Item()
                                                                       .Text("Confidential information enclosed")
                                                                       .AlignCenter()
                                                                       .SemiBold().FontSize(20).FontColor(Colors.Black);

                                                                      x.Item()
                                                                       .Text("Current as of " + DateTime.Now.ToShortDateString())
                                                                       .AlignCenter()
                                                                       .Light().FontSize(20).FontColor(Colors.Black);
                                                                      x.Item()
                                                                       .Text("DRAFT ONLY not to be used for production")
                                                                       .AlignCenter()
                                                                       .FontSize(20).FontColor(Colors.Black);

                                                                      x.Item()
                                                                       .Text(show.Name)
                                                                       .AlignCenter()
                                                                       .SemiBold().FontSize(48).FontColor(Colors.Black);
                                                                      x.Item().PageBreak();
                                                                      ShowInfo(x.Item(), show);
                                                                      x.Item()
                                                                       .Text(show.Name)
                                                                       .AlignCenter()
                                                                       .SemiBold().FontSize(28).FontColor(Colors.Black);
                                                                      x.Item()
                                                                       .Text("Cue List")
                                                                       .AlignCenter()
                                                                       .Light().FontSize(20).FontColor(Colors.Black);
                                                                      CueList(x.Item(), cues);
                                                                      x.Item().PageBreak();
                                                                      Runsheet(x.Item(), cues, tasks, roles);
                                                                  });

                                                      PageFooter(page);
                                                  });
                               })
                       .GeneratePdf();
    }

    private static void PageFooter(PageDescriptor page)
    {
        page.Footer()
            .AlignCenter()
            .ShowIf(context => context.PageNumber > 1) // Hides on first page
            .Text(x =>
                  {
                      x.Span("Page ");
                      x.CurrentPageNumber();
                  });
    }

    private static void ShowInfo(IContainer container, Show show)
    {
        container.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                                                    {
                                                        columns.RelativeColumn();
                                                        columns.RelativeColumn();
                                                    });

                            table.Cell().Padding(8).Text($"{show.Name}").FontSize(24).Bold();
                            table.Cell().Padding(8).Text("");

                            DateTime start = show.Start;
                            DateTime end   = show.End;

                            table.Cell().Padding(8).Text("Date");
                            table.Cell().Padding(8).Text($"{start.ToLongDateString()}");
                            table.Cell().Padding(8).Text("Start");
                            table.Cell().Padding(8).Text($"{start.ToShortTimeString()}");
                            table.Cell().Padding(8).Text("End");
                            table.Cell().Padding(8).Text($"{end.ToShortTimeString()}");
                        });
    }

    private static void CueList(IContainer container, Cue[] cues)
    {
        container.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                                                    {
                                                        columns.ConstantColumn(50);
                                                        columns.RelativeColumn();
                                                    });

                            table.Header(header =>
                                         {
                                             header.Cell().BorderBottom(2).Padding(8).Text("#");
                                             header.Cell().BorderBottom(2).Padding(8).Text("Name");
                                         });

                            foreach (Cue cue in cues)
                            {
                                table.Cell().Padding(8).Text($"{cue.Position}");
                                table.Cell().Padding(8).Text(cue.Name);
                            }
                        });
    }

    private static void Runsheet(IContainer container, Cue[] cues, CueTask[] tasks, Role[] roles)
    {
        Dictionary<int, string> roleNames = [];
        foreach (Role role in roles)
        {
            roleNames.Add(role.Id, role.Name);
        }

        container.Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                                                    {
                                                        columns.ConstantColumn(40);
                                                        columns.RelativeColumn();
                                                    });

                            foreach (Cue cue in cues)
                            {
                                table.Cell()
                                     .AlignMiddle()
                                     .Padding(8)
                                     .Text($"{cue.Position}");
                                table.Cell().Row(r =>
                                                 {
                                                     r.RelativeItem()
                                                      .Padding(8)
                                                      .AlignMiddle()
                                                      .Text(cue.Name)
                                                      .FontSize(16);
                                                     r.RelativeItem(3)
                                                      .Padding(8)
                                                      .Column(column =>
                                                              {
                                                                  column.Spacing(4);

                                                                  foreach (CueTask task in tasks.Where(t => t.CueId == cue.Id))
                                                                  {
                                                                      string taskText = task.RoleId is { } roleId &&
                                                                                        roleNames.TryGetValue(roleId, out string? roleName)
                                                                                            ? $"{roleName}: {task.Tasks}"
                                                                                            : task.Tasks;

                                                                      column.Item().Row(taskRow =>
                                                                                        {
                                                                                            taskRow.ConstantItem(14)
                                                                                                   .AlignMiddle()
                                                                                                   .Svg(TaskIconSvg(task.Icon))
                                                                                                   .FitArea();

                                                                                            taskRow.RelativeItem()
                                                                                                   .AlignMiddle()
                                                                                                   .Text(taskText)
                                                                                                   .FontSize(10);
                                                                                        });
                                                                  }
                                                              });
                                                 });
                            }
                        });
    }

    private static string TaskIconSvg(CueTaskIcon icon) =>
        $"""
         <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24">
         {CueTask.CueTaskIconToString(icon)}
         </svg>
         """;
}