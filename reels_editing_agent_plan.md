# Reel Editing Fix Plan (Agent Handoff)

## Objective
Fix Reel Editing so that:
- Thumbnail editing is removed from the UI/workflow.
- Mock background music data is reliably available in DB.
- Saving **Crop Dimensions** and **Background Music** produces real visible/audible changes (not just DB metadata updates).

## Current Problems (from code)
1. `Save` appears to work but media does not change because:
   - `VideoProcessingService` is a stub and returns original path unchanged.
   - Feed playback (`ReelItemView`) ignores `CropDataJson` and `BackgroundMusicId`.
2. Thumbnail editing still exists in UI and ViewModel, but should be removed.
3. Music seed data exists only behind `IF (COUNT(*) = 0)`; this is fragile for partially-seeded DBs.

## Scope
- In scope: Reels Editing + Reels Feed integration for crop/music effect playback.
- Out of scope: full ffmpeg transcoding pipeline.

---

## Implementation Strategy
Use **runtime playback effects** (WinUI) instead of transcoding files:
- Persist crop/music metadata in DB as today.
- Apply crop visually at playback time in feed/editor from `CropDataJson`.
- Play background music from `MusicTrack.AudioUrl` in sync with video, using saved start/duration/volume.

This gives immediate user-visible behavior after Save with low risk and no external ffmpeg dependency.

---

## Work Breakdown

### 1) Remove Thumbnail Editing
**Owner files**
- `Features/ReelsEditing/Views/ReelsEditingPage.xaml`
- `Features/ReelsEditing/Views/ReelsEditingPage.xaml.cs`
- `Features/ReelsEditing/ViewModels/ReelsEditingViewModel.cs`
- `Features/ReelsEditing/Models/VideoEditMetadata.cs`

**Tasks**
- Remove `Thumbnail` option button and thumbnail panel from editor UI.
- Remove thumbnail-specific code-behind:
  - `ThumbnailPreviewPlayer`
  - `SelectThumbnailFrameButton_Click`
  - `SyncSavedThumbnailSelection`
  - thumbnail visibility/load handling.
- Remove `SaveThumbnailCommand`/`SaveThumbnailAsync` from `ReelsEditingViewModel`.
- Remove `ThumbnailFrameSeconds` from active edit flow.
- Keep backward compatibility when parsing old JSON:
  - Ignore existing `thumbnailTimeSec` if present.

**Done when**
- No thumbnail controls appear in editor.
- Build passes with no thumbnail command/binding errors.

---

### 2) Make Music Seed Data Reliable
**Owner file**
- `Core/Database/DatabaseInitializer.cs`

**Tasks**
- Replace one-shot seed block for `MusicTrack` with idempotent per-track insert:
  - `IF NOT EXISTS (...) INSERT ...` per track name (or URL).
- Ensure at least one reel has `BackgroundMusicId` for demo verification:
  - Add safe update seed for sample reel(s) where `BackgroundMusicId IS NULL`.
- Keep SQL idempotent (safe across many app launches).

**Done when**
- Existing DBs without full music rows get backfilled.
- Music selection dialog always has tracks in normal dev setups.

---

### 3) Apply Crop + Music on Playback (Actual Effect)
**Owner files**
- `Features/ReelsFeed/Views/ReelItemView.xaml`
- `Features/ReelsFeed/Views/ReelItemView.xaml.cs`
- `Features/ReelsEditing/ViewModels/ReelsEditingViewModel.cs`
- `Features/ReelsEditing/Services/AudioLibraryService.cs` (read path reuse)
- `Core/Models/ReelModel.cs` (only if extra resolved properties are needed)

**Tasks**
- Parse persisted crop/music metadata (`CropDataJson`) in feed item view.
- Crop visual effect:
  - Apply a clip/transform based on saved x/y/width/height (mapped to display size).
  - Reset transform/clip to default when metadata absent.
- Music effect:
  - Resolve `BackgroundMusicId -> MusicTrack.AudioUrl`.
  - Start separate audio playback when reel starts.
  - Apply saved `musicStartTime`, `musicDuration`, `musicVolume`.
  - Stop/pause/dispose audio player with reel pause/unload/dispose.
- Ensure current save flow still persists:
  - `SaveCropAsync` persists crop metadata.
  - `SaveMusicAsync` persists selected track + music params.
- Fix save behavior edge case:
  - if reel already has a saved `BackgroundMusicId`, load selected track on reel load so user can adjust start/duration/volume and save.

**Done when**
- After saving crop/music, reopening reel/feed clearly reflects changes.
- Crop is visually different, music is audible and follows saved params.

---

### 4) Editor UX Consistency
**Owner files**
- `Features/ReelsEditing/Views/ReelsEditingPage.xaml`
- `Features/ReelsEditing/ViewModels/ReelsEditingViewModel.cs`

**Tasks**
- Keep only two options: `Crop Dimensions`, `Background Music`.
- Ensure save status messages differentiate success/failure clearly.
- Ensure no silent no-op when pressing Save Music:
  - Show explicit validation message if no track selected.

---

## Verification Checklist
1. Run app, open Reel Editing, select reel.
2. Confirm no thumbnail UI exists.
3. Adjust crop margins, Save, navigate away and back, confirm crop still applied.
4. Choose music, set start/duration/volume, Save, play reel in feed, confirm music overlay.
5. Restart app, confirm persisted behavior remains.
6. Validate DB row changed:
   - `Reel.CropDataJson` updated
   - `Reel.BackgroundMusicId` updated
   - `Reel.LastEditedAt` updated

---

## Suggested Commit Order
1. Remove thumbnail feature.
2. Make DB music seed idempotent.
3. Implement playback crop/music application.
4. Fix Save Music edge cases + UX messaging.
5. Manual verification pass.

---

## Risks / Notes
- Mapping crop pixels to rendered control size must handle varying aspect ratios; test with multiple clips.
- Audio must be disposed carefully to avoid WinUI/COM teardown issues (follow existing media disposal style in `ReelItemView`).
- Do not rely on `IVideoProcessingService` stub for visible behavior.
