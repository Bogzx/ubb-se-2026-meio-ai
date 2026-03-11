### 1. Formal Requirements
*   **Requirement[s]:** 
    *   The system must allow an authenticated user to select a previously uploaded reel for editing.
    *   The system must provide functionality for the user to define crop dimensions (x/y coordinates, width, and height) for the selected reel.
    *   The system must allow the user to select a background music track from an available library and apply it to their reel.
    *   The system must persist the edited reel states (crop metadata and selected music track) in the long-term database to ensure edits are not lost across sessions.
*   **Owner:** Beatrice
*   **Cross-Team Dependencies:** 
    *   **Database Team:** Needed to update the Reel schema to accommodate crop metadata and relations to an audio/music table.
    *   **UI Team:** Needed to design the video editing layout (cropping overlays, music selection modal, preview player).
    *   **Backend Logic Team:** Needed to build the video-processing logic (handling the visual crop and merging the audio track) before saving the finalized media to storage.

### 2. Diagram Blueprint
*   **Use Case Diagram Additions:** 
    *   *Actor:* Authenticated User
    *   *Use Cases:* "Select Uploaded Reel", "Crop Reel Video", "Add Background Music", "Save Reel Edits"
*   **Database Schema Additions:** 
    *   *Tables:* `Reel`, `MusicTrack`
    *   *Columns in `Reel`:* `ReelID` (PK), `UserID` (FK), `VideoResourceURL` (String), `CropData` (JSON/String - storing x, y, width, height), `BackgroundMusicID` (FK, nullable), `LastEditedAt` (DateTime).
    *   *Columns in `MusicTrack`:* `MusicTrackID` (PK), `TrackName` (String), `AudioResourceURL` (String), `DurationInSeconds` (Int).
    *   *Relationships:* 
        *   `Reel` has an optional Many-to-One relationship with `MusicTrack` (Many reels can use the same specific background music track).
        *   `Reel` has a Many-to-One relationship with `User`.
*   **Class Diagram (MVVM) Additions:** 
    *   *Models:* `Reel`, `MusicTrack`, `VideoEditMetadata`
    *   *Views:* `ReelGalleryView` (for choosing the reel to edit), `ReelEditorView` (the main crop/music workspace), `MusicSelectionDialogView` (the library pop-up).
    *   *ViewModels:* `ReelGalleryViewModel`, `ReelEditorViewModel`, `MusicSelectionDialogViewModel`
    *   *Utils/Services:* `IVideoProcessingService` (for handling media crop/merge), `IAudioLibraryService` (to fetch available music).

### 3. Project Management Tasks (Max 30-Minutes Each)

**Database & Models**
*   **Task:** Design Reel Entity Data Model Update
    *   **Description:** Create the formal model specification for the `Reel` entity, ensuring it includes properties for `CropData` (to hold crop coordinates) and a relationship linking it to the user. Max 30 mins effort.
*   **Task:** Design MusicTrack Entity Data Model
    *   **Description:** Create the formal model specification for the `MusicTrack` entity containing ID, Track Name, and URL fields. Max 30 mins effort.
*   **Task:** Map Foreign Key Relationship: Reel to MusicTrack
    *   **Description:** Update the database schema design to implement a nullable `BackgroundMusicID` on the `Reel` table, officially linking the altered reels to the selected audio tracks. Max 30 mins effort.
*   **Task:** Design VideoEditMetadata Wrapper Model
    *   **Description:** Create a local data model `VideoEditMetadata` to temporarily hold the user's ongoing edits (current crop coordinates, tentative music selection) before they hit save. Max 30 mins effort.

**Backend Services & ViewModels**
*   **Task:** Scaffold ReelEditorViewModel
    *   **Description:** Create the `ReelEditorViewModel` class container. Define observable properties for `SelectedReel`, `CurrentCropCoordinates`, and `SelectedMusicTrack`. Max 30 mins effort.
*   **Task:** Create Save Edits Command in ReelEditorViewModel
    *   **Description:** Implement a `SaveEditsCommand` within the `ReelEditorViewModel` that will eventually collect the current state variables and pass them to a processing service. Max 30 mins effort.
*   **Task:** Define IVideoProcessingService Contract
    *   **Description:** Design the interface `IVideoProcessingService` containing method signatures for `ApplyCrop()` and `MergeAudioTrack()`. (No implementation code, just the interface). Max 30 mins effort.
*   **Task:** Define IAudioLibraryService Contract
    *   **Description:** Design the interface `IAudioLibraryService` outlining a `GetAvailableTracks()` method to fetch standard background music. Max 30 mins effort.
*   **Task:** Scaffold MusicSelectionDialogViewModel
    *   **Description:** Create the `MusicSelectionDialogViewModel`. Inject `IAudioLibraryService` and create an observable list of `MusicTrack` models to populate the dialog view. Max 30 mins effort.
*   **Task:** Create SelectTrack Command
    *   **Description:** Inside `MusicSelectionDialogViewModel`, implement a command that captures the user's clicked track and passes the ID back to the parent `ReelEditorViewModel`. Max 30 mins effort.

**GUI (Views)**
*   **Task:** Mockup ReelGalleryView Layout
    *   **Description:** Design the structural layout definition for `ReelGalleryView` to display a grid of the user's existing historical reels ready for editing. Max 30 mins effort.
*   **Task:** Mockup ReelEditorView: Media Player Area
    *   **Description:** Construct the layout structural definition for the center of `ReelEditorView` to hold the video preview playback component. Max 30 mins effort.
*   **Task:** Mockup ReelEditorView: Cropping Interface
    *   **Description:** Add UI specifications (like sliders or a bounded rectangle overlay) to `ReelEditorView` to simulate the visual cropping experience. Bind to ViewModel coordinates. Max 30 mins effort.
*   **Task:** Mockup ReelEditorView: Music Action Bar
    *   **Description:** Design a bottom-action-bar layout for `ReelEditorView` containing an "Add Music" button and a placeholder label for the currently active track name. Max 30 mins effort.
*   **Task:** Mockup MusicSelectionDialogView Layout
    *   **Description:** Design a pop-up modal or bottom-sheet layout to act as `MusicSelectionDialogView`, containing a scrollable list of tracks and a "Confirm" button. Max 30 mins effort.
