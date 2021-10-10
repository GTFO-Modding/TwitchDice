using System;
using System.Collections.Generic;
using System.Text;

//namespace TwitchDice.Utilities
//{
//    public class TimedEventManager
//    {
//		PUI_Inventory inventory = GuiManager.Current.m_playerLayer.Inventory;
//		PUI_InventoryIconDisplay iconDisplay = inventory.m_iconDisplay;
//			foreach (RectTransform rectTransform in iconDisplay.GetComponentsInChildren<RectTransform>(true))
//			{
//				bool flag2 = rectTransform.name == "Background Fade";
//				if (flag2)
//				{
//					rectTransform.gameObject.SetActive(true);
//					this.TextMesh = null;
//					foreach (KeyValuePair<InventorySlot, PUI_InventoryItem> keyValuePair in inventory.m_inventorySlots)
//					{
//						bool flag3 = this.TextMesh == null;
//						if (flag3)
//						{
//							this.TextMesh = UnityEngine.Object.Instantiate<TextMeshPro>(keyValuePair.Value.m_slim_archetypeName);
//							break;
//						}
//}
//GameObject gameObject = new GameObject("TimerShowObject")
//{
//	layer = 5,
//	hideFlags = HideFlags.HideAndDontSave
//};
//gameObject.transform.SetParent(rectTransform.transform, false);
//this.TextMesh.transform.SetParent(gameObject.transform, false);
//RectTransform component = this.TextMesh.GetComponent<RectTransform>();
//component.anchoredPosition = new Vector2(-5f, 8f);
//this.TextMesh.SetText(this._endskill ? "EndskillTimer" : "NotEndskillTimer");
//this.TextMesh.ForceMeshUpdate();
//				}
//			}
//    }
//}
