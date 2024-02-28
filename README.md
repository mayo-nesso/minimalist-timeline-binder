# README: MinimalistTLBinder

Plugin to bind objects to a `PlayableAsset` used on Timelines and load your Timeline in runtime.

## Motivation

By default, a `PlayableAsset` can hold references to elements that coexist in the same scope as them.

Let's suppose that we have a `Timeline` that has a reference to the *Main Camera* of the scene. If this `Timeline` exists on the scene the reference will be there and the timeline will be able to manipulate the referenced camera.

Now, if we move the `Timeline` to a prefab that will be loaded at runtime, the  *Main Camera* reference will be lost. To solve this issue we would have to bind the element by code.

This plugin helps with the process of binding elements in runtime.

## How to use it

To make it work, we have to create our `Timeline` and assign the references on Edit mode, then we will use the plugin to wire the objects that are referenced on it.

And we should be ready to go.

## Detailed steps

1.- Work on a Timeline and add object references to it, as usual.
So a timeline like this:

![Timeline example](./README_Assets/readme_1.png?raw=true)

Also, you can see on Unity Inspector the current bindings:

![Timeline example](./README_Assets/readme_2.png?raw=true)

2.- Add a `PlayableDirectorBinder` component and assign the `PlaybleDirector` component to it on the `Director` field.

![Timeline example](./README_Assets/readme_3.png?raw=true)

3.- Wire the elements using the button that says `Wire bindings!` on the `PlayableDirectorBinder` component.

![Timeline example](./README_Assets/readme_4.png?raw=true)

This will do two things;

- First, Add a `BindableAsset` component to each of the referenced objects. This new component will hold a unique identifier and a reference to the bound object.
- Second, update the names of each one of the `tracks` on the `Timeline` to include that unique identifier (later this name will be parsed to look for the object with that unique Id).

Updated track names:

![Updated Track Names](./README_Assets/readme_5.png?raw=true)

New `BindableAsset` component:

![Updated Track Names](./README_Assets/readme_6.png?raw=true)

4.- Save your changes and load your Timeline in runtime. By default, `PlayableDirectorBinder` will try to bind elements on `Awake` but this option can be disabled (this would require a manual invocation of `SearchAndBindObjectsOnScene()` on `PlayableDirectorBinder`).

## Other actions

There are other buttons on the `PlayableDirectorBinder` component besides *Wire bindings!*. Those are useful for testing or cleaning up things:

- Wire bindings!: Add a `BindableAsset` component to referenced objects and update the track name accordingly. If there is already a `BindableAsset` component on the referenced object, it will just update the track name. In the case that the unique Id on the `BindableAsset` doesn't match the specified Id on the track name, **the track name will be updated**.
- Search and bind scene objects: Search on the current scene and try to bind elements using the unique Id specified on the track name.
- Cleanup binding references: Unbind all the references. Useful if, after Wire the bindings you want to see if everything is working. (ie; First *Wire...* the bindings, then *Cleanup...*, then *Search and bind...*. If you are in the scene where the timeline will be loaded, all the references should be re-bound).
- Reset track names with assigned components: Reset the name on all the tracks that have an assigned component. So you always can wire and the track names should go back to their previous values.
- Reset \*ALL\* track names: Rest the name on all tracks, without filtering those that have or have no references. If you reset the name of a track that has nothing assigned to it, it will be on you to reassign a valid reference.

## Known issues

- The process to obtain the `BindableAsset` references relies on *Finding Objects Of Type All* that exist on a loaded scene (`Resources.FindObjectsOfTypeAll<BindableAsset>().Where(ba => ba.gameObject.scene.isLoaded);`). This can cause issues with the `PlayableDirectorBinder` option `Try Bind on Awake` if the `PlayableDirectorBinder` is being instantiated at the same time. In other words, the current way to look up `BindableAsset` cannot retrieve them during `Awake` if they are also being instantiated. So make sure that you have the required `BindableAsset` elements already loaded before loading a `PlayableDirectorBinder`. A workaround for this is to manually invoke the `SearchAndBindObjectsOnScene()` method after the scene is loaded with both elements.

## Future improvements

- Improve the lookup function; `FindObjectsOfTypeAll` can be expensive.
- Improve assignment function: Currently, after retrieving all the `BindableAsset` we try to bind each one of them. A better solution would be to a dictionary.
