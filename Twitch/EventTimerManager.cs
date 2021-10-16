using System;
using System.Collections.Generic;
using System.Text;
using TwitchDice.Utilities;
using UnityEngine;

namespace TwitchDice.Twitch
{
    public class EventTimerManager : MonoBehaviour
    {
        public EventTimerManager(IntPtr intPtr) : base(intPtr) 
        {
            if (Instance == null)
                Instance = this;
            else
                Log.Error("Created a duplicate TimedEventManager!");
        }
        public static EventTimerManager Instance { get; private set; }

        public void Awake()
        {
            Hooks.InventorySlotsUpdated += RecalculateAll;
        }

        private readonly List<EventTimer> ActiveTimedEvents = new List<EventTimer>();

        public void RemoveTimedInstance(EventTimer eventTimer)
        {
            ActiveTimedEvents.Remove(eventTimer);
            RecalculateAll();
        }

        public void AddTimedInstance(IDiceEvent diceEvent)
        {
            if (CheckIfEventIsActive(diceEvent)) return;

            PUI_Inventory inventory = GuiManager.PlayerLayer.Inventory;
            PUI_InventoryItem timer = inventory.AddSlotItem();
            EventTimer eventTimer = timer.gameObject.AddComponent<EventTimer>();
            eventTimer.Init(timer, diceEvent);
            ActiveTimedEvents.Add(eventTimer);

            timer.name = $"Event Timer: {diceEvent.EventID}";

            timer.SetState(ePUI_InventortyItemState.Slim);

            timer.m_slim_root.transform.Find("Pivot/Background Small").gameObject.SetActive(false);
            timer.m_slim_root.transform.Find("Pivot/Infinite ammo").gameObject.SetActive(false);

            RecalculateAll();
            Log.Debug($"Added new timed event with name {diceEvent.EventName}");
        }

        private bool CheckIfEventIsActive(IDiceEvent diceEvent)
        {
            foreach (var activeEvent in ActiveTimedEvents)
            {
                if (activeEvent.EventID == diceEvent.EventID)
                {
                    activeEvent.ResetTime();
                    return true;
                }
            }
            return false;
        }

        private float CalcVanilla(PUI_Inventory inventory)
        {
            float startPos = inventory.m_invSlotStartPos.y;

            //Calc starting height
            for (int i = 0; i < inventory.m_slotGUIOrder.Length; i++)
            {
                PUI_InventoryItem pui_InventoryItem = inventory.m_inventorySlots[inventory.m_slotGUIOrder[i]];
                if (pui_InventoryItem.gameObject.activeSelf)
                {
                    startPos -= inventory.m_invSlotStartOffsetY + pui_InventoryItem.CurrentHeight;
                }
            }
            return startPos;
        }

        private void RecalculateAll()
        {
            PUI_Inventory inventory = GuiManager.PlayerLayer.Inventory;
            float start = CalcVanilla(inventory);
            foreach (var timer in ActiveTimedEvents)
            {
                RecalculatePosition(inventory, timer.Item, start);
            }
        }

        private void RecalculatePosition(PUI_Inventory inventory, PUI_InventoryItem timer, float start)
        {
            foreach (var activeEvent in ActiveTimedEvents)
            {
                if (activeEvent.Item == timer) break;
                start -= inventory.m_invSlotStartOffsetY + activeEvent.Item.CurrentHeight;
            }
            Log.Debug($"Set position of event {timer.name} to {start}");
            timer.SetPosition(new Vector2(inventory.m_invSlotStartPos.x, start));
        }
    }

    public class EventTimer : MonoBehaviour
    {
        public EventTimer(IntPtr intPtr) : base(intPtr) 
        {
        }
        public PUI_InventoryItem Item;
        public float Time;
        public string EventName;
        public string EventID;
        private DateTime End;
        private bool setup = false;
        private string color;

        public void Init(PUI_InventoryItem item, IDiceEvent diceEvent)
        {
            Item = item;
            Time = diceEvent.Time;
            EventName = diceEvent.EventName;
            EventID = diceEvent.EventID;
            item.name += EventName;

            End = DateTime.Now.AddSeconds(Time);
            setup = true;
            color = ColorUtil.GetDiceColorForTier(diceEvent.Tier);
        }

        public void ResetTime()
        {
            End = DateTime.Now.AddSeconds(Time);
        }

        void Update()
        {
            if (setup == false) return;
            TimeSpan remainingTime = End.Subtract(DateTime.Now);
            string remaining = remainingTime.ToString(@"ss\.ff");

            if (remainingTime.TotalSeconds > 0)
            {
                Item.SetArchetypeName($"<color={color}>{EventName}</color>: {remaining}");
            } else
            {
                EventTimerManager.Instance.RemoveTimedInstance(this);
                Destroy(transform.gameObject);
                Destroy(Item);
            }
        }
    }
}
