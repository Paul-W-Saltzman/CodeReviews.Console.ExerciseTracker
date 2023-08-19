﻿using ExerciseTrackerCarDioLogics.Helpers;
using ExerciseTrackerCarDioLogics.Models;
using Spectre.Console;
using static ExerciseTrackerCarDioLogics.Helpers.Enums;

namespace ExerciseTrackerCarDioLogics;

public class UserInterface
{
    DependencyInjectorMethods dependencyInjectorMethods = new DependencyInjectorMethods();
    ExSessionController controller;
    DateTime dateStart, dateEnd;
    TimeSpan duration;
    string comment;

    public void StartMenu() //Depending on what the user chooses it will "inject" either the EF or RawSQL.
    {
        bool isAppRunning = true;

        do
        {
            Console.Clear();

            var choice = AnsiConsole.Prompt(
                 new SelectionPrompt<ExerciseType>()
                .Title("Select an exercise type")
                .AddChoices(ExerciseType.Weigths, ExerciseType.Cardio, ExerciseType.Exit));

            switch (choice)
            {
                case ExerciseType.Weigths:
                    controller = dependencyInjectorMethods.EFImplementationService();
                    MainMenu();
                    break;
                case ExerciseType.Cardio:
                    controller = dependencyInjectorMethods.RawSQLImplementationService();
                    MainMenu();
                    break;
                case ExerciseType.Exit:
                    isAppRunning = false;
                    break;
            }
        } while (isAppRunning == true);
    }

    public void MainMenu()
    {
        int id;
        bool isMainMenuRunning = true;

        do
        {
            Console.Clear();

            var choice = AnsiConsole.Prompt(
                 new SelectionPrompt<MainMenuOptions>()
                .Title("Select an exercise type")
                .AddChoices(MainMenuOptions.AddSession, MainMenuOptions.RemoveSession,
                            MainMenuOptions.UpdateSession, MainMenuOptions.ViewSessions, MainMenuOptions.BackToStartMenu));

            switch (choice)
            {
                case MainMenuOptions.AddSession:
                    GetUserInput(out dateStart, out dateEnd, out duration, out comment);
                    controller.AddExSession(dateStart, dateEnd, duration, comment);
                    break;
                case MainMenuOptions.RemoveSession:
                    ShowListSessions();
                    Console.WriteLine("Removing Session...");
                    id = GetSessionID();
                    controller.RemoveSession(id);
                    break;
                case MainMenuOptions.UpdateSession:
                    ShowListSessions();
                    Console.WriteLine("Updating Session...");
                    id = GetSessionID();
                    ExSession session = controller.GetSessionById(id);

                    if(session != null)
                    {
                        GetUserInput(out dateStart, out dateEnd, out duration, out comment);
                        controller.UpdateExSession(id, dateStart, dateEnd, duration, comment, session);
                    }
                    break;
                case MainMenuOptions.ViewSessions:
                    ShowListSessions();
                    break;
                case MainMenuOptions.BackToStartMenu:
                    isMainMenuRunning = false;
                    break;
            }
        } while (isMainMenuRunning == true);
    }

    public void ShowListSessions()
    {
        List<ExSession> sessions = controller.GetAllSessions();

        var table = new Table();
        table.AddColumn("ID");
        table.AddColumn("Start Date");
        table.AddColumn("End Date");
        table.AddColumn("Duration");
        table.AddColumn("Comment");

        foreach (var session in sessions)
        {
            table.AddRow(
                session.Id.ToString(),
                session.DateStart.ToString("yyyy-MM-dd HH:mm"),
                session.DateEnd.ToString("yyyy-MM-dd HH:mm"),
                session.Duration.ToString(),
                session.Comment);
        }

        AnsiConsole.Render(table);

        Console.WriteLine("Press any key to continue");
        Console.ReadLine();
    }

    public int GetSessionID()
    {
        List<ExSession> sessions = controller.GetAllSessions();

        var sessionOptions = new List<int>();

        foreach (var session in sessions)
        {
            sessionOptions.Add(session.Id);
        }

        var sessionSelection = AnsiConsole.Prompt(
            new SelectionPrompt<int>()
                .Title("Choose a session:")
                .AddChoices(sessionOptions));

        return sessionSelection;
    }

    public void GetUserInput(out DateTime dateStart, out DateTime dateEnd, out TimeSpan duration, out string comment)
    {
        bool isDateValid;

        do
        {
            Console.WriteLine("Start Date:");
            string input = AnsiConsole.Prompt(new TextPrompt<string>("Enter date and time(yyyy/MM/dd HH:mm):"));
            dateStart = Validator.GetValidDate(input, out isDateValid);
        } while (isDateValid != true);

        do
        {
            Console.WriteLine("End Date:");
            string input = AnsiConsole.Prompt(new TextPrompt<string>("Enter date and time (yyyy/MM/dd HH:mm):"));
            dateEnd = Validator.GetValidDate(input, out isDateValid);
        } while (isDateValid != true);

        duration = dateEnd - dateStart;

        comment = AnsiConsole.Ask<string>("Write any comment:");
    }
}
