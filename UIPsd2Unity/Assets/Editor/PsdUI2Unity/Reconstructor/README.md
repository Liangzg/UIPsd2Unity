Extending the PSD Reconstructor
===

The PSD Importer can be extended to reconstruct the PSD Layouts for other UI systems, such as NGUI.

To create an extension, create a C# class that implements the interface `IPsdConstructor`. Put the extension class somewhere in an `Editor` directory and when the PSD Importer is opened, there will be additional options found in the `Sprite Creation` foldout.

Implementations
---

[`IPsdConstructor.cs`](IPsdConstructor.cs) is heavily commented and explains what the functions should return.

[`SpriteConstructor.cs`](SpriteConstructor.cs) is the most straightforward implementation, pay attention to the functions `GetLayerPosition` and `GetGroupPosition` as they explain what those functions do.

[`UiImgConstructor.cs`](UiImgConstructor.cs) is an implementation for Unity's UI system which is a bit more complex because of how it relies on the hierarchy to sort. Pay attention to `AddComponents` and `HandleGroupClose` as well as the reconstruction loop covered in the next section.

Reconstruction loop
---

The following is pseudo code for how the PSD is recreated. This is found in [`PsdBuilder.cs`](PsdBuilder.cs)

```
// In this loop, the PSD is recreated without regard
// for the chosen alignment
for (lowestLayer to highestLayer)
{
  if (layer is inside a layer group)
  {
    if (start or end of layer group)
    {
      if (start)
      {
        IPsdConstructor.CreatGameObject();
        IPsdConstructor.HandleGroupOpen();
        set as current layer group
      }
      else
      {
        IPsdConstructor.HandleGroupClose();
        Add to layer group list
      }
      continue;
    }
  } //

  // Visible image layers
  IPsdConstructor.CreatGameObject();
  Add layer to current layer group

  IPsdConstructor.AddComponents();
  IPsdConstructor.GetLayerPosition();
}

// This loop repositions the PSD so that layer groups
// are positioned according to chosen alignment
foreach (layer group in layer group list)
{
  IPsdConstructor.GetGroupPosition();
  // Create a "copy" of the layer group
  IPsdConstructor.CreateGameObject()
  // 1. Apply the layer group position, with regards to alignment
  // 2. Copy all the children in the original layer group to new layer group
  // 3. Delete old layer group
}
```
