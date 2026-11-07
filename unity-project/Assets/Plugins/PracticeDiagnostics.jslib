mergeInto(LibraryManager.library, {
  Practice6Report: function(frame, x, y, distance, ar) {
    window.practice6Diagnostics = {frame:frame, centerX:x, centerY:y, distance:distance, ar:!!ar};
  }
});
