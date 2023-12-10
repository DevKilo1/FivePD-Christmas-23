using System;
using System.Dynamic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CitizenFX.Core;
using CitizenFX.Core.Native;
using FivePD_HostageScenarioCallout;
using FivePD.API;
using FivePD.API.Utils;

namespace FivePD_Christmas_23;

[Guid("EB46F32D-6EC2-4B0F-B924-A78D56D9CF6B")]
[CalloutProperties("Christmas Public Disturbance", "DevKilo", "1.0")]
public class PublicDisturbance : Callout
{
    private Ped elf, victim;
    private Blip victimBlip, elfBlip;
    private bool ready = false;
    private bool pedsIdentified = false;
    private bool stopArguing = false;
    private bool qbcore = false;
    public static EventHandlerDictionary eventHandlers;

    public PublicDisturbance()
    {
        InitInfo(GetLocation());
        ShortName = "Public Disturbance";
        CalloutDescription = "A man claiming to be an elf has been spotted lurking about the Davis High School.";
        ResponseCode = 3;
        StartDistance = 50f;
    }

    private Vector3 GetLocation()
    {
        Vector3 randomLoc = Utils.GetRandomLocationInRadius(Game.PlayerPed.Position, 100, 850);
        Ped closestPed = Utils.GetClosestPed(randomLoc, 20f, true);
        if (closestPed != null)
        {
            return closestPed.Position;
        }
        else
        {
            if (randomLoc == Vector3.Zero)
                return GetLocation(); // This could exhaust client
            else
                return randomLoc; // This could exhaust client
        }
    }

    private async Task<Ped> SpawnElf()
    {
        Ped ped = await Utils.SpawnPedOneSync(PedHash.FreemodeMale01, Location, true);
        if (ped == null) return await SpawnElf();
        API.SetPedComponentVariation(ped.Handle, 8, 15, 0, 0);
        API.SetPedComponentVariation(ped.Handle, 11, 195, 0, 0);
        API.SetPedComponentVariation(ped.Handle, 4, 39, 0, 0);
        API.SetPedComponentVariation(ped.Handle, 6, 3, 7, 0);
        API.SetPedPropIndex(ped.Handle, 0, 23, 0, true);
        return ped;
    }

    public override async Task OnAccept()
    {
        AcceptHandler();
    }

    private async Task AcceptHandler()
    {
        elf = await SpawnElf();
        victim = await Utils.SpawnPedOneSync(Utils.GetRandomPed(), Utils.GetRandomLocationInRadius(Location, 2, 3),
            true);
        while (victim.Gender == Gender.Male) // TO-DO: Add male
        {
            victim.Delete();
            victim = await Utils.SpawnPedOneSync(Utils.GetRandomPed(), Utils.GetRandomLocationInRadius(Location, 2, 3),
                true);
            await BaseScript.Delay(100);
        }

        if (elf.Position.DistanceTo(victim.Position) > 3f)
        {
            Vector3 newPos = Utils.GetRandomLocationInRadius(victim.Position, 2, 3);
            if (elf.Position.DistanceTo(victim.Position) > 20f)
                elf.Position = newPos;
            Utils.KeepTaskGoToForPed(elf, newPos, 0.5f);
            await Utils.WaitUntilEntityIsAtPos(elf, newPos, 0.5f);
        }

        if (victim.Position.DistanceTo(Location) > StartDistance && elf.Position.DistanceTo(Location) > StartDistance)
            if (victim.Position.DistanceTo(elf.Position) < StartDistance)
            {
                Location = victim.Position;
            }
            else
            {
                victim.Position = Location;
                elf.Position = Utils.GetRandomLocationInRadius(victim.Position, 2, 3);
            }

        victim.Task.TurnTo(elf);
        elf.Task.TurnTo(victim);
        InitBlip();
        await BaseScript.Delay(3000);
        PedsWaitScenarioBeforeStart();
        ready = true;
    }

    private async Task PedsWaitScenarioBeforeStart()
    {
        Utils.StopKeepTaskPlayAnimation(elf);
        Utils.StopKeepTaskPlayAnimation(victim);
        await Utils.RequestAnimDict("anim@amb@nightclub@lazlow@hi_dancefloor@");
        await Utils.RequestAnimDict("friends@fra@ig_1");
        await Utils.RequestAnimDict("mini@prostitutestalk");
        await Utils.RequestAnimDict("anim@heists@ornate_bank@chat_manager");
        await Utils.RequestAnimDict("mini@darts");
        Utils.StopKeepTaskPlayAnimation(elf);
        Utils.StopKeepTaskPlayAnimation(victim);
        elf.Task.PlayAnimation("anim@amb@nightclub@lazlow@hi_dancefloor@", "crowddance_mi_17_talking_laz");
        Utils.KeepTaskPlayAnimation(elf, "anim@amb@nightclub@lazlow@hi_dancefloor@", "crowddance_mi_17_talking_laz");
        victim.Task.PlayAnimation("friends@fra@ig_1", "impatient_idle_a", 8f, 8f, -1, AnimationFlags.Loop, 1f);
        Utils.KeepTaskPlayAnimation(victim, "friends@fra@ig_1", "impatient_idle_a");
        while (!Started)
            await BaseScript.Delay(100);
        Utils.StopKeepTaskPlayAnimation(elf);
        Utils.StopKeepTaskPlayAnimation(victim);
    }

    public override void OnStart(Ped closest)
    {
        StartHandler();
    }

    private async Task ArgueConversation()
    {
        while (!stopArguing)
        {
            Debug.WriteLine("d");
            if (stopArguing) return;
            Utils.StopKeepTaskPlayAnimation(victim);
            Utils.StopKeepTaskPlayAnimation(elf);
            victim.Task.TurnTo(elf);
            await BaseScript.Delay(1000);
            Utils.KeepTaskPlayAnimation(victim, "mini@prostitutestalk", "street_argue_f_a");
            await SubtitleChat(victim, "Why can't you just go away?", 7, 145, 224);
            if (stopArguing) return;
            Utils.StopKeepTaskPlayAnimation(elf);
            Utils.KeepTaskPlayAnimation(elf, "anim@heists@ornate_bank@chat_manager", "wear_weird");
            await SubtitleChat(elf, "I have your gift!", 89, 222, 71);
            if (stopArguing) return;
            SubtitleChat(elf, "You won't take it!", 89, 222, 71);
            await SubtitleChat(victim, "You're weird, dude! Why would I want to follow you?", 7, 145, 224);
            if (stopArguing) return;
            await SubtitleChat(elf, "My van is just around the corner! I just want to give you my special gift!", 89,
                222,
                71);
            if (stopArguing) return;
            SubtitleChat(victim, "Ew! You creep!", 7, 145, 224);
            await BaseScript.Delay(500);
            if (stopArguing) return;
            await SubtitleChat(elf, "You don't understand! Look at me.", 89, 222, 71);
            if (stopArguing) return;
            await SubtitleChat(victim, "And what are you supposed to be?", 7, 145, 224);
            if (stopArguing) return;
            SubtitleChat(elf, "I'm an elf!", 89, 222, 71);
            if (stopArguing) return;
            await BaseScript.Delay(500);
            await SubtitleChat(victim, "Santa is not real, so are his elves. Everybody knows that!", 7, 145, 224);
            if (stopArguing) return;
            await BaseScript.Delay(500);
            if (stopArguing) return;
            await SubtitleChat(elf, "I am real!", 89, 222, 71);

            Utils.StopKeepTaskPlayAnimation(victim);
            Utils.StopKeepTaskPlayAnimation(elf);

            PedsWaitScenarioBeforeStart();
            await BaseScript.Delay(30000);
        }
    }

    private async Task StartHandler()
    {
        while (!ready)
            await BaseScript.Delay(100);
        ShowDialog("Investigate the ~y~disturbance~s~", 5000, StartDistance + 2f);
        // TO-DO: Female starts arguing with male.
        ArgueConversation();
        // 
        BlipHandler();
        Utils.StopKeepTaskPlayAnimation(elf);
        Utils.StopKeepTaskPlayAnimation(victim);
        await Utils.WaitUntilEntityIsAtPos(Game.PlayerPed, victim.Position, 2f);
        stopArguing = true;
        Utils.ImmediatelyStop3DText();
        Utils.StopKeepTaskPlayAnimation(elf);
        Utils.StopKeepTaskPlayAnimation(victim);
        victim.Task.ClearAll();
        victim.Task.TurnTo(Game.PlayerPed);
        elf.Task.ClearAll();
        elf.Task.TurnTo(Game.PlayerPed);
        SubtitleChat(victim, "Ugh, finally! Some authority here.", 7, 145, 224);
        await BaseScript.Delay(1000);
        victim.Task.PlayAnimation("mini@prostitutestalk", "street_argue_f_b", 8f, 8f, -1, AnimationFlags.Loop, 1f);
        Utils.KeepTaskPlayAnimation(victim, "friends@fra@ig_1", "impatient_idle_a");
        // TO-DO: Start Conversation
        Utils.KeepTaskPlayAnimation(elf, "anim@mp_player_intcelebrationmale@face_palm", "face_palm");
        SubtitleChat(elf, "Officer, this is a big misunderstanding.", 89, 222, 71);
        await BaseScript.Delay(500);
        await SubtitleChat(victim, "Thank you for coming.", 7, 145, 224);
        Utils.KeepTaskPlayAnimation(elf, "mini@darts", "wait_idle");
        await SubtitleChat(victim, "He won't leave me alone.", 7, 145, 224);
        await SubtitleChat(victim, "I just want him to leave so I can get on with my day.", 7, 145, 224);
        await SubtitleChat(victim, "He won't stop talking about giving me a gift.", 7, 145, 224);
        SubtitleChat(victim, "The fool thinks he's Santa's elf.", 7, 145, 224);
        await BaseScript.Delay(1000);
        SubtitleChat(elf, "I am his elf!", 89, 222, 71);
        await SubtitleChat(victim, "Be quiet over there.", 7, 145, 224);
        await SubtitleChat(victim, "That's everything.", 7, 145, 224);
        // Create object and declare options
        var interaction =
            new Utils.DecisionInteraction(["To Elf: Now you can explain.", "To Elf: Just get out of here."]);
        // Connect a new function (Action) to the choice's index based on that array you sent.
        // It will run that function when the option is selected.
        interaction.Connect(0, new Action(async () =>
        {
            // To Elf: Now you can explain.
            Debug.WriteLine("Running thing");
            await SubtitleChat(elf, "I really am Santa's Elf!", 89, 222, 71);
            var interaction2 = new Utils.DecisionInteraction([
                "No you aren't. Santa isn't real.", "Sure.", "I suppose you couldn't prove it if you were."
            ]);
            interaction2.Connect(0,new Action(async () =>
            {
                await SubtitleChat(Game.PlayerPed, "Come on buddy. Santa isn't real.");
            }));
            
            interaction2.Connect(1,new Action(async () =>
            {
                await SubtitleChat(Game.PlayerPed, "Sure.");
            }));
            interaction2.Connect(2, new Action(async () =>
            {
                await SubtitleChat(Game.PlayerPed, "I suppose you couldn't prove it if you were...");
            }));
        }));
    }


    private async Task SubtitleChat(Entity entity, string chat, int red = 255, int green = 255, int blue = 255,
        int opacity = 255)
    {
        int time = chat.Length * 150;
        Utils.Draw3DText(new Vector3(entity.Position.X, entity.Position.Y, entity.Position.Z + 1f), chat, 0.5f, time,
            red, green, blue, opacity);
        await BaseScript.Delay(time);
    }

    private async Task BlipHandler()
    {
        HandleVictimBlip();
        elfBlip = await BlipPedWhenPlayerCanSee(elf, BlipColor.Green, "Elf?");
        if (victimBlip != null)
            pedsIdentified = true;
    }


    private async Task HandleVictimBlip()
    {
        victimBlip = await BlipPedWhenPlayerCanSee(victim, BlipColor.MichaelBlue, "Victim");
    }

    private async Task<Blip> BlipPedWhenPlayerCanSee(Ped ped, BlipColor blipColor = BlipColor.Red,
        string blipName = "")
    {
        await Utils.WaitUntilPedCanSeePed(Game.PlayerPed, ped);
        Blip blip = ped.AttachBlip();
        blip.Color = blipColor;
        blip.Name = blipName;
        return blip;
    }

    public override void OnCancelBefore()
    {
        if (victimBlip != null)
        {
            victimBlip.Delete();
            victimBlip = null;
        }

        if (elfBlip != null)
        {
            elfBlip.Delete();
            elfBlip = null;
        }

        API.RemoveAnimDict("anim@amb@nightclub@lazlow@hi_dancefloor@");
        API.RemoveAnimDict("friends@fra@ig_1");
        API.RemoveAnimDict("mini@prostitutestalk");
        API.RemoveAnimDict("anim@heists@ornate_bank@chat_manager");
        API.RemoveAnimDict("mini@darts");

        base.OnCancelBefore();
    }

    public override Task<bool> CheckRequirements()
    {
        bool result = DateTime.Today.Month == 12;
        return Task.FromResult(result);
    }
}

public class script : BaseScript
{
    public script()
    {
        PublicDisturbance.eventHandlers = EventHandlers;
        EventHandlers["KiloFivePD:QBCore:GiveItem"] += QBGiveItem;
    }

    private void QBGiveItem(string itemName)
    {
    }
}