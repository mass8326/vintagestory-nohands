using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;

namespace NoHands {
  public class NoHandsSystem : ModSystem {
    ICoreClientAPI Api;
    IClientPlayer Player => Api.World.Player;
    IPlayerInventoryManager InventoryManager => Player.InventoryManager;
    ItemSlot ActiveSlot => InventoryManager.ActiveHotbarSlot;
    ItemSlot OffhandSlot => Player.Entity.LeftHandItemSlot;

    public override void StartClientSide(ICoreClientAPI api) {
      base.StartClientSide(api);
      Api = api;
      Api.Input.RegisterHotKey("nohands-clear-active", Lang.Get("nohands:hotkey-clear-active"), GlKeys.R, HotkeyType.InventoryHotkeys);
      Api.Input.SetHotKeyHandler("nohands-clear-active", (KeyCombination keyCombination) => {
        if (ActiveSlot != null) return ClearSlot(ActiveSlot);
        else return false;
      });
      Api.Input.RegisterHotKey("nohands-clear-both", Lang.Get("nohands:hotkey-clear-both"), GlKeys.R, HotkeyType.InventoryHotkeys, false, false, true);
      Api.Input.SetHotKeyHandler("nohands-clear-both", (KeyCombination keyCombination) => {
        var activeSuccess = ActiveSlot != null ? ClearSlot(ActiveSlot) : false;
        var offhandSuccess = OffhandSlot != null ? ClearSlot(OffhandSlot) : false;
        return activeSuccess || offhandSuccess;
      });
    }

    private bool ClearSlot(ItemSlot slot) {
      if (slot.Empty) return false;
      var operation = new ItemStackMoveOperation(
        Api.World,
        EnumMouseButton.None,
        EnumModifierKey.SHIFT,
        EnumMergePriority.AutoMerge
      );
      var packets = InventoryManager.TryTransferAway(slot, ref operation, true, true);
      if (packets == null) return false;
      foreach (object packet in packets)
        Api.Network.SendPacketClient(packet);
      return true;
    }
  }
}
