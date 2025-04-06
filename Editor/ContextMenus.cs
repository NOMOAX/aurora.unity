using System;
using Aurora.Pooling;
using Aurora.Unity.UI;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace Aurora.UnityEditor
{
    /// <summary>
    /// <see cref="ContextMenu"/>s.
    /// </summary>
    internal static class ContextMenus
    {
        private const string Context = "CONTEXT";

        private const string DeleteUselessProperties = "Delete Useless Properties";

        private static int GetComponentIndex(Component component)
        {
            if (component == null)
            {
                throw new ArgumentNullException(nameof(component));
            }
            var components = PredefinedPools<Component>.List.Get();
            try
            {
                component.GetComponents(components);
                return components.IndexOf(component);
            }
            finally
            {
                PredefinedPools<Component>.List.Return(components);
            }
        }

        private static void SetComponentIndex(Component component, int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index), index, null);
            }
            int currentIndex;
            var components = PredefinedPools<Component>.List.Get();
            try
            {
                component.GetComponents(components);
                if (index >= components.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index), index, null);
                }
                currentIndex = components.IndexOf(component);
            }
            finally
            {
                PredefinedPools<Component>.List.Return(components);
            }
            if (currentIndex < index)
            {
                for (var i = 0; i < index - currentIndex; i++)
                {
                    ComponentUtility.MoveComponentDown(component);
                }
            }
            else if (currentIndex > index)
            {
                for (var i = 0; i < currentIndex - index; i++)
                {
                    ComponentUtility.MoveComponentUp(component);
                }
            }
        }

        private static T ReplaceComponent<T>(Component replaced) where T : Component
        {
            var componentIndex = GetComponentIndex(replaced);

            var gameObject = replaced.gameObject;
            Undo.DestroyObjectImmediate(replaced);
            var t = Undo.AddComponent<T>(gameObject);

            Undo.RecordObject(gameObject, "set component index");
            SetComponentIndex(t, componentIndex);

            return t;
        }

        private static T ReplaceBehaviour<T>(Behaviour replaced) where T : Behaviour
        {
            var enabled = replaced.enabled;

            var t = ReplaceComponent<T>(replaced);

            Undo.RecordObject(t, $"set {nameof(Behaviour.enabled)}");
            t.enabled = enabled;

            return t;
        }

        private static T ReplaceGraphic<T>(Graphic replaced) where T : Graphic
        {
            var color          = replaced.color;
            var material       = replaced.material != Graphic.defaultGraphicMaterial ? replaced.material : null;
            var raycastTarget  = replaced.raycastTarget;
            var raycastPadding = replaced.raycastPadding;

            var t = ReplaceBehaviour<T>(replaced);

            Undo.RecordObject(t, $"set {nameof(Graphic.color)}");
            t.color = color;
            Undo.RecordObject(t, $"set {nameof(Graphic.material)}");
            t.material = material;
            Undo.RecordObject(t, $"set {nameof(Graphic.raycastTarget)}");
            t.raycastTarget = raycastTarget;
            Undo.RecordObject(t, $"set {nameof(Graphic.raycastPadding)}");
            t.raycastPadding = raycastPadding;

            return t;
        }

        private static T ReplaceMaskableGraphic<T>(MaskableGraphic replaced) where T : MaskableGraphic
        {
            var maskable = replaced.maskable;

            var t = ReplaceGraphic<T>(replaced);

            Undo.RecordObject(t, $"set {nameof(MaskableGraphic.maskable)}");
            t.maskable = maskable;

            return t;
        }

        #region Button

        [MenuItem(Context + "/" + nameof(Button) + "/" + "Replace with " + nameof(EnhancedButton))]
        private static void ReplaceButtonWithEnhancedButton(MenuCommand menuCommand)
        {
            var button = menuCommand.context as Button;
            if (button == null)
            {
                return;
            }

            var interactable = button.interactable;

            var enhancedButton = ReplaceBehaviour<EnhancedButton>(button);

            Undo.RecordObject(enhancedButton, $"set {nameof(EnhancedButton.Interactable)}");
            enhancedButton.Interactable = interactable;
        }

        #endregion

        #region EnhancedButton

        [MenuItem(Context + "/" + nameof(EnhancedButton) + "/" + "Replace with " + nameof(Button))]
        private static void ReplaceEnhancedButtonWithButton(MenuCommand menuCommand)
        {
            var enhancedButton = menuCommand.context as EnhancedButton;
            if (enhancedButton == null)
            {
                return;
            }

            var interactable = enhancedButton.Interactable;

            var button = ReplaceBehaviour<Button>(enhancedButton);

            Undo.RecordObject(button, $"set {nameof(Button.interactable)}");
            button.interactable = interactable;
        }

        #endregion

        #region Image

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceImageWithRawImage(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            var texture = image.sprite != null ? image.sprite.texture : null;

            var rawImage = ReplaceMaskableGraphic<RawImage>(image);

            Undo.RecordObject(rawImage, $"set {nameof(RawImage.texture)}");
            rawImage.texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceImageWithClear(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(image);
        }

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceImageWithBlock(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(image);
        }

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceImageWithCircle(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            var texture = image.sprite != null ? image.sprite.texture : null;

            var circle = ReplaceMaskableGraphic<Circle>(image);

            Undo.RecordObject(circle, $"set {nameof(Circle.Texture)}");
            circle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceImageWithAnnulus(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            var texture = image.sprite != null ? image.sprite.texture : null;

            var annulus = ReplaceMaskableGraphic<Annulus>(image);

            Undo.RecordObject(annulus, $"set {nameof(Annulus.Texture)}");
            annulus.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceImageWithRoundedRectangle(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            var texture = image.sprite != null ? image.sprite.texture : null;

            var roundedRectangle = ReplaceMaskableGraphic<RoundedRectangle>(image);

            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.Texture)}");
            roundedRectangle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(RoundedRectangleBorder))]
        private static void ReplaceImageWithRoundedRectangleBorder(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            var texture = image.sprite != null ? image.sprite.texture : null;

            var roundedRectangleBorder = ReplaceMaskableGraphic<RoundedRectangleBorder>(image);

            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.Texture)}");
            roundedRectangleBorder.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Image) + "/" + "Replace with " + nameof(CustomGraphic))]
        private static void ReplaceImageWithCustomGraphic(MenuCommand menuCommand)
        {
            var image = menuCommand.context as Image;
            if (image == null)
            {
                return;
            }

            var texture = image.sprite != null ? image.sprite.texture : null;

            var customGraphic = ReplaceMaskableGraphic<CustomGraphic>(image);

            Undo.RecordObject(customGraphic, $"set {nameof(CustomGraphic.Texture)}");
            customGraphic.Texture = texture;
        }

        #endregion

        #region RawImage

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceRawImageWithImage(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(rawImage);
        }

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceRawImageWithClear(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(rawImage);
        }

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceRawImageWithBlock(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(rawImage);
        }

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceRawImageWithCircle(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            var texture = rawImage.texture;

            var circle = ReplaceMaskableGraphic<Circle>(rawImage);

            Undo.RecordObject(circle, $"set {nameof(Circle.Texture)}");
            circle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceRawImageWithAnnulus(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            var texture = rawImage.texture;

            var annulus = ReplaceMaskableGraphic<Annulus>(rawImage);

            Undo.RecordObject(annulus, $"set {nameof(Annulus.Texture)}");
            annulus.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceRawImageWithRoundedRectangle(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            var texture = rawImage.texture;

            var roundedRectangle = ReplaceMaskableGraphic<RoundedRectangle>(rawImage);

            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.Texture)}");
            roundedRectangle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(RoundedRectangleBorder))]
        private static void ReplaceRawImageWithRoundedRectangleBorder(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            var texture = rawImage.texture;

            var roundedRectangleBorder = ReplaceMaskableGraphic<RoundedRectangleBorder>(rawImage);

            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.Texture)}");
            roundedRectangleBorder.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RawImage) + "/" + "Replace with " + nameof(CustomGraphic))]
        private static void ReplaceRawImageWithCustomGraphic(MenuCommand menuCommand)
        {
            var rawImage = menuCommand.context as RawImage;
            if (rawImage == null)
            {
                return;
            }

            var texture = rawImage.texture;

            var customGraphic = ReplaceMaskableGraphic<CustomGraphic>(rawImage);

            Undo.RecordObject(customGraphic, $"set {nameof(CustomGraphic.Texture)}");
            customGraphic.Texture = texture;
        }

        #endregion

        #region Clear

        [MenuItem(Context + "/" + nameof(Clear) + "/" + DeleteUselessProperties)]
        private static void DeleteClearUselessProperties(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            Undo.RecordObject(clear, $"set {nameof(Graphic.color)}");
            clear.color = Color.white;
            Undo.RecordObject(clear, $"set {nameof(Graphic.material)}");
            clear.material = null;
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceClearWithImage(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(clear);
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceClearWithRawImage(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<RawImage>(clear);
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceClearWithBlock(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(clear);
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceClearWithCircle(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Circle>(clear);
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceClearWithAnnulus(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Annulus>(clear);
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceClearWithRoundedRectangle(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<RoundedRectangle>(clear);
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(RoundedRectangleBorder))]
        private static void ReplaceClearWithRoundedRectangleBorder(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<RoundedRectangleBorder>(clear);
        }

        [MenuItem(Context + "/" + nameof(Clear) + "/" + "Replace with " + nameof(CustomGraphic))]
        private static void ReplaceClearWithCustomGraphic(MenuCommand menuCommand)
        {
            var clear = menuCommand.context as Clear;
            if (clear == null)
            {
                return;
            }

            ReplaceMaskableGraphic<CustomGraphic>(clear);
        }

        #endregion

        #region Block

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceBlockWithImage(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(block);
        }

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceBlockWithRawImage(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<RawImage>(block);
        }

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceBlockWithClear(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(block);
        }

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceBlockWithCircle(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Circle>(block);
        }

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceBlockWithAnnulus(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Annulus>(block);
        }

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceBlockWithRoundedRectangle(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<RoundedRectangle>(block);
        }

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(RoundedRectangleBorder))]
        private static void ReplaceBlockWithRoundedRectangleBorder(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<RoundedRectangleBorder>(block);
        }

        [MenuItem(Context + "/" + nameof(Block) + "/" + "Replace with " + nameof(CustomGraphic))]
        private static void ReplaceBlockWithCustomGraphic(MenuCommand menuCommand)
        {
            var block = menuCommand.context as Block;
            if (block == null)
            {
                return;
            }

            ReplaceMaskableGraphic<CustomGraphic>(block);
        }

        #endregion

        #region Circle

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceCircleWithImage(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(circle);
        }

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceCircleWithRawImage(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            var texture = circle.Texture;

            var rawImage = ReplaceMaskableGraphic<RawImage>(circle);

            Undo.RecordObject(rawImage, $"set {nameof(RawImage.texture)}");
            rawImage.texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceCircleWithClear(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(circle);
        }

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceCircleWithBlock(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(circle);
        }

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceCircleWithAnnulus(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            var texture = circle.Texture;

            var annulus = ReplaceMaskableGraphic<Annulus>(circle);

            Undo.RecordObject(annulus, $"set {nameof(Annulus.Texture)}");
            annulus.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceCircleWithRoundedRectangle(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            var texture = circle.Texture;

            var roundedRectangle = ReplaceMaskableGraphic<RoundedRectangle>(circle);

            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.Texture)}");
            roundedRectangle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(RoundedRectangleBorder))]
        private static void ReplaceCircleWithRoundedRectangleBorder(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            var texture = circle.Texture;

            var roundedRectangleBorder = ReplaceMaskableGraphic<RoundedRectangleBorder>(circle);

            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.Texture)}");
            roundedRectangleBorder.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Circle) + "/" + "Replace with " + nameof(CustomGraphic))]
        private static void ReplaceCircleWithCustomGraphic(MenuCommand menuCommand)
        {
            var circle = menuCommand.context as Circle;
            if (circle == null)
            {
                return;
            }

            var texture = circle.Texture;

            var customGraphic = ReplaceMaskableGraphic<CustomGraphic>(circle);

            Undo.RecordObject(customGraphic, $"set {nameof(CustomGraphic.Texture)}");
            customGraphic.Texture = texture;
        }

        #endregion

        #region Annulus

        [MenuItem(Context + "/" + nameof(Annulus) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceAnnulusWithImage(MenuCommand menuCommand)
        {
            var annulus = menuCommand.context as Annulus;
            if (annulus == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(annulus);
        }

        [MenuItem(Context + "/" + nameof(Annulus) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceAnnulusWithRawImage(MenuCommand menuCommand)
        {
            var annulus = menuCommand.context as Annulus;
            if (annulus == null)
            {
                return;
            }

            var texture = annulus.Texture;

            var rawImage = ReplaceMaskableGraphic<RawImage>(annulus);

            Undo.RecordObject(rawImage, $"set {nameof(RawImage.texture)}");
            rawImage.texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Annulus) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceAnnulusWithClear(MenuCommand menuCommand)
        {
            var annulus = menuCommand.context as Annulus;
            if (annulus == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(annulus);
        }

        [MenuItem(Context + "/" + nameof(Annulus) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceAnnulusWithBlock(MenuCommand menuCommand)
        {
            var annulus = menuCommand.context as Annulus;
            if (annulus == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(annulus);
        }

        [MenuItem(Context + "/" + nameof(Annulus) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceAnnulusWithCircle(MenuCommand menuCommand)
        {
            var annulus = menuCommand.context as Annulus;
            if (annulus == null)
            {
                return;
            }

            var texture = annulus.Texture;

            var circle = ReplaceMaskableGraphic<Circle>(annulus);

            Undo.RecordObject(circle, $"set {nameof(Circle.Texture)}");
            circle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Annulus) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceAnnulusWithRoundedRectangle(MenuCommand menuCommand)
        {
            var annulus = menuCommand.context as Annulus;
            if (annulus == null)
            {
                return;
            }

            var texture = annulus.Texture;

            var roundedRectangle = ReplaceMaskableGraphic<RoundedRectangle>(annulus);

            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.Texture)}");
            roundedRectangle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(Annulus) + "/" + "Replace with " + nameof(CustomGraphic))]
        private static void ReplaceAnnulusWithCustomGraphic(MenuCommand menuCommand)
        {
            var annulus = menuCommand.context as Annulus;
            if (annulus == null)
            {
                return;
            }

            var texture = annulus.Texture;

            var customGraphic = ReplaceMaskableGraphic<CustomGraphic>(annulus);

            Undo.RecordObject(customGraphic, $"set {nameof(CustomGraphic.Texture)}");
            customGraphic.Texture = texture;
        }

        #endregion

        #region RoundedRectangle

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceRoundedRectangleWithImage(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(roundedRectangle);
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceRoundedRectangleWithRawImage(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            var texture = roundedRectangle.Texture;

            var rawImage = ReplaceMaskableGraphic<RawImage>(roundedRectangle);

            Undo.RecordObject(rawImage, $"set {nameof(RawImage.texture)}");
            rawImage.texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceRoundedRectangleWithClear(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(roundedRectangle);
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceRoundedRectangleWithBlock(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(roundedRectangle);
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceRoundedRectangleWithCircle(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            var texture = roundedRectangle.Texture;

            var circle = ReplaceMaskableGraphic<Circle>(roundedRectangle);

            Undo.RecordObject(circle, $"set {nameof(Circle.Texture)}");
            circle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceRoundedRectangleWithAnnulus(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            var texture = roundedRectangle.Texture;

            var annulus = ReplaceMaskableGraphic<Annulus>(roundedRectangle);

            Undo.RecordObject(annulus, $"set {nameof(Annulus.Texture)}");
            annulus.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(RoundedRectangleBorder))]
        private static void ReplaceRoundedRectangleWithRoundedRectangleBorder(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            var texture                           = roundedRectangle.Texture;
            var segments                          = roundedRectangle.Segments;
            var topLeftCornerRadiusNormalized     = roundedRectangle.topLeftCornerRadiusNormalized;
            var topLeftCornerRadius               = roundedRectangle.topLeftCornerRadius;
            var topRightCornerRadiusNormalized    = roundedRectangle.topRightCornerRadiusNormalized;
            var topRightCornerRadius              = roundedRectangle.topRightCornerRadius;
            var bottomLeftCornerRadiusNormalized  = roundedRectangle.bottomLeftCornerRadiusNormalized;
            var bottomLeftCornerRadius            = roundedRectangle.bottomLeftCornerRadius;
            var bottomRightCornerRadiusNormalized = roundedRectangle.bottomRightCornerRadiusNormalized;
            var bottomRightCornerRadius           = roundedRectangle.bottomRightCornerRadius;

            var roundedRectangleBorder = ReplaceMaskableGraphic<RoundedRectangleBorder>(roundedRectangle);

            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.Texture)}");
            roundedRectangleBorder.Texture = texture;
            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.Segments)}");
            roundedRectangleBorder.Segments = segments;
            Undo.RecordObject(
                roundedRectangleBorder,
                $"set {nameof(RoundedRectangleBorder.topLeftCornerRadiusNormalized)}"
            );
            roundedRectangleBorder.TopLeftCornerRadiusNormalized = topLeftCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.topLeftCornerRadius)}");
            roundedRectangleBorder.TopLeftCornerRadius = topLeftCornerRadius;
            Undo.RecordObject(
                roundedRectangleBorder,
                $"set {nameof(RoundedRectangleBorder.topRightCornerRadiusNormalized)}"
            );
            roundedRectangleBorder.TopRightCornerRadiusNormalized = topRightCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.topRightCornerRadius)}");
            roundedRectangleBorder.TopRightCornerRadius = topRightCornerRadius;
            Undo.RecordObject(
                roundedRectangleBorder,
                $"set {nameof(RoundedRectangleBorder.bottomLeftCornerRadiusNormalized)}"
            );
            roundedRectangleBorder.BottomLeftCornerRadiusNormalized = bottomLeftCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.bottomLeftCornerRadius)}");
            roundedRectangleBorder.BottomLeftCornerRadius = bottomLeftCornerRadius;
            Undo.RecordObject(
                roundedRectangleBorder,
                $"set {nameof(RoundedRectangleBorder.bottomRightCornerRadiusNormalized)}"
            );
            roundedRectangleBorder.BottomRightCornerRadiusNormalized = bottomRightCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.bottomRightCornerRadius)}");
            roundedRectangleBorder.BottomRightCornerRadius = bottomRightCornerRadius;
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangle) + "/" + "Replace with " + nameof(CustomGraphic))]
        private static void ReplaceRoundedRectangleWithCustomGraphic(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as RoundedRectangle;
            if (roundedRectangle == null)
            {
                return;
            }

            var texture = roundedRectangle.Texture;

            var customGraphic = ReplaceMaskableGraphic<CustomGraphic>(roundedRectangle);

            Undo.RecordObject(customGraphic, $"set {nameof(CustomGraphic.Texture)}");
            customGraphic.Texture = texture;
        }

        #endregion

        #region RoundedRectangleBorder

        [MenuItem(Context + "/" + nameof(RoundedRectangleBorder) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceRoundedRectangleBorderWithImage(MenuCommand menuCommand)
        {
            var roundedRectangleBorder = menuCommand.context as RoundedRectangleBorder;
            if (roundedRectangleBorder == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(roundedRectangleBorder);
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangleBorder) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceRoundedRectangleBorderWithRawImage(MenuCommand menuCommand)
        {
            var roundedRectangleBorder = menuCommand.context as RoundedRectangleBorder;
            if (roundedRectangleBorder == null)
            {
                return;
            }

            var texture = roundedRectangleBorder.Texture;

            var rawImage = ReplaceMaskableGraphic<RawImage>(roundedRectangleBorder);

            Undo.RecordObject(rawImage, $"set {nameof(RawImage.texture)}");
            rawImage.texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangleBorder) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceRoundedRectangleBorderWithClear(MenuCommand menuCommand)
        {
            var roundedRectangleBorder = menuCommand.context as RoundedRectangleBorder;
            if (roundedRectangleBorder == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(roundedRectangleBorder);
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangleBorder) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceRoundedRectangleBorderWithBlock(MenuCommand menuCommand)
        {
            var roundedRectangleBorder = menuCommand.context as RoundedRectangleBorder;
            if (roundedRectangleBorder == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(roundedRectangleBorder);
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangleBorder) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceRoundedRectangleBorderWithCircle(MenuCommand menuCommand)
        {
            var roundedRectangleBorder = menuCommand.context as RoundedRectangleBorder;
            if (roundedRectangleBorder == null)
            {
                return;
            }

            var texture = roundedRectangleBorder.Texture;

            var circle = ReplaceMaskableGraphic<Circle>(roundedRectangleBorder);

            Undo.RecordObject(circle, $"set {nameof(Circle.Texture)}");
            circle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangleBorder) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceRoundedRectangleBorderWithAnnulus(MenuCommand menuCommand)
        {
            var roundedRectangleBorder = menuCommand.context as RoundedRectangleBorder;
            if (roundedRectangleBorder == null)
            {
                return;
            }

            var texture = roundedRectangleBorder.Texture;

            var annulus = ReplaceMaskableGraphic<Annulus>(roundedRectangleBorder);

            Undo.RecordObject(annulus, $"set {nameof(Annulus.Texture)}");
            annulus.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(RoundedRectangleBorder) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceRoundedRectangleBorderWithRoundedRectangle(MenuCommand menuCommand)
        {
            var roundedRectangleBorder = menuCommand.context as RoundedRectangleBorder;
            if (roundedRectangleBorder == null)
            {
                return;
            }

            var texture                           = roundedRectangleBorder.texture;
            var segments                          = roundedRectangleBorder.Segments;
            var topLeftCornerRadiusNormalized     = roundedRectangleBorder.topLeftCornerRadiusNormalized;
            var topLeftCornerRadius               = roundedRectangleBorder.topLeftCornerRadius;
            var topRightCornerRadiusNormalized    = roundedRectangleBorder.topRightCornerRadiusNormalized;
            var topRightCornerRadius              = roundedRectangleBorder.topRightCornerRadius;
            var bottomLeftCornerRadiusNormalized  = roundedRectangleBorder.bottomLeftCornerRadiusNormalized;
            var bottomLeftCornerRadius            = roundedRectangleBorder.bottomLeftCornerRadius;
            var bottomRightCornerRadiusNormalized = roundedRectangleBorder.bottomRightCornerRadiusNormalized;
            var bottomRightCornerRadius           = roundedRectangleBorder.bottomRightCornerRadius;

            var roundedRectangle = ReplaceMaskableGraphic<RoundedRectangle>(roundedRectangleBorder);

            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.Texture)}");
            roundedRectangle.Texture = texture;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.Segments)}");
            roundedRectangle.Segments = segments;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.topLeftCornerRadiusNormalized)}");
            roundedRectangle.TopLeftCornerRadiusNormalized = topLeftCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.topLeftCornerRadius)}");
            roundedRectangle.TopLeftCornerRadius = topLeftCornerRadius;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.topRightCornerRadiusNormalized)}");
            roundedRectangle.TopRightCornerRadiusNormalized = topRightCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.topRightCornerRadius)}");
            roundedRectangle.TopRightCornerRadius = topRightCornerRadius;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.bottomLeftCornerRadiusNormalized)}");
            roundedRectangle.BottomLeftCornerRadiusNormalized = bottomLeftCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.bottomLeftCornerRadius)}");
            roundedRectangle.BottomLeftCornerRadius = bottomLeftCornerRadius;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.bottomRightCornerRadiusNormalized)}");
            roundedRectangle.BottomRightCornerRadiusNormalized = bottomRightCornerRadiusNormalized;
            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.bottomRightCornerRadius)}");
            roundedRectangle.BottomRightCornerRadius = bottomRightCornerRadius;
        }

        #endregion

        #region CustomGraphic

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(Image))]
        private static void ReplaceCustomGraphicWithImage(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as CustomGraphic;
            if (roundedRectangle == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Image>(roundedRectangle);
        }

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(RawImage))]
        private static void ReplaceCustomGraphicWithRawImage(MenuCommand menuCommand)
        {
            var roundedRectangle = menuCommand.context as CustomGraphic;
            if (roundedRectangle == null)
            {
                return;
            }

            var texture = roundedRectangle.Texture;

            var rawImage = ReplaceMaskableGraphic<RawImage>(roundedRectangle);

            Undo.RecordObject(rawImage, $"set {nameof(RawImage.texture)}");
            rawImage.texture = texture;
        }

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(Clear))]
        private static void ReplaceCustomGraphicWithClear(MenuCommand menuCommand)
        {
            var customGraphic = menuCommand.context as CustomGraphic;
            if (customGraphic == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Clear>(customGraphic);
        }

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(Block))]
        private static void ReplaceCustomGraphicWithBlock(MenuCommand menuCommand)
        {
            var customGraphic = menuCommand.context as CustomGraphic;
            if (customGraphic == null)
            {
                return;
            }

            ReplaceMaskableGraphic<Block>(customGraphic);
        }

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(Circle))]
        private static void ReplaceCustomGraphicWithCircle(MenuCommand menuCommand)
        {
            var customGraphic = menuCommand.context as CustomGraphic;
            if (customGraphic == null)
            {
                return;
            }

            var texture = customGraphic.Texture;

            var circle = ReplaceMaskableGraphic<Circle>(customGraphic);

            Undo.RecordObject(circle, $"set {nameof(Circle.Texture)}");
            circle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(Annulus))]
        private static void ReplaceCustomGraphicWithAnnulus(MenuCommand menuCommand)
        {
            var customGraphic = menuCommand.context as CustomGraphic;
            if (customGraphic == null)
            {
                return;
            }

            var texture = customGraphic.Texture;

            var annulus = ReplaceMaskableGraphic<Annulus>(customGraphic);

            Undo.RecordObject(annulus, $"set {nameof(Annulus.Texture)}");
            annulus.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(RoundedRectangle))]
        private static void ReplaceCustomGraphicWithRoundedRectangle(MenuCommand menuCommand)
        {
            var customGraphic = menuCommand.context as CustomGraphic;
            if (customGraphic == null)
            {
                return;
            }

            var texture = customGraphic.Texture;

            var roundedRectangle = ReplaceMaskableGraphic<RoundedRectangle>(customGraphic);

            Undo.RecordObject(roundedRectangle, $"set {nameof(RoundedRectangle.Texture)}");
            roundedRectangle.Texture = texture;
        }

        [MenuItem(Context + "/" + nameof(CustomGraphic) + "/" + "Replace with " + nameof(RoundedRectangleBorder))]
        private static void ReplaceCustomGraphicWithRoundedRectangleBorder(MenuCommand menuCommand)
        {
            var customGraphic = menuCommand.context as CustomGraphic;
            if (customGraphic == null)
            {
                return;
            }

            var texture = customGraphic.Texture;

            var roundedRectangleBorder = ReplaceMaskableGraphic<RoundedRectangleBorder>(customGraphic);

            Undo.RecordObject(roundedRectangleBorder, $"set {nameof(RoundedRectangleBorder.Texture)}");
            roundedRectangleBorder.Texture = texture;
        }

        #endregion

        private static T ReplaceLayoutGroup<T>(LayoutGroup replaced) where T : LayoutGroup
        {
            var padding        = replaced.padding;
            var childAlignment = replaced.childAlignment;

            var t = ReplaceBehaviour<T>(replaced);

            Undo.RecordObject(t, $"set {nameof(LayoutGroup.padding)}");
            t.padding = padding;
            Undo.RecordObject(t, $"set {nameof(LayoutGroup.childAlignment)}");
            t.childAlignment = childAlignment;

            return t;
        }

        private static T ReplaceHorizontalOrVerticalLayoutGroup<T>(HorizontalOrVerticalLayoutGroup replaced)
            where T : HorizontalOrVerticalLayoutGroup
        {
            var spacing                = replaced.spacing;
            var childForceExpandWidth  = replaced.childForceExpandWidth;
            var childForceExpandHeight = replaced.childForceExpandHeight;
            var childControlWidth      = replaced.childControlWidth;
            var childControlHeight     = replaced.childControlHeight;
            var childScaleWidth        = replaced.childScaleWidth;
            var childScaleHeight       = replaced.childScaleHeight;
            var reverseArrangement     = replaced.reverseArrangement;

            var t = ReplaceLayoutGroup<T>(replaced);

            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.spacing)}");
            t.spacing = spacing;
            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.childForceExpandWidth)}");
            t.childForceExpandWidth = childForceExpandWidth;
            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.childForceExpandHeight)}");
            t.childForceExpandHeight = childForceExpandHeight;
            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.childControlWidth)}");
            t.childControlWidth = childControlWidth;
            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.childControlHeight)}");
            t.childControlHeight = childControlHeight;
            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.childScaleWidth)}");
            t.childScaleWidth = childScaleWidth;
            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.childScaleHeight)}");
            t.childScaleHeight = childScaleHeight;
            Undo.RecordObject(t, $"set {nameof(HorizontalOrVerticalLayoutGroup.reverseArrangement)}");
            t.reverseArrangement = reverseArrangement;

            return t;
        }

        #region HorizontalLayoutGroup

        [MenuItem(Context + "/" + nameof(HorizontalLayoutGroup) + "/" + "Replace with " + nameof(VerticalLayoutGroup))]
        private static void ReplaceHorizontalLayoutGroupWithVerticalLayoutGroup(MenuCommand menuCommand)
        {
            var horizontalLayoutGroup = menuCommand.context as HorizontalLayoutGroup;
            if (horizontalLayoutGroup == null)
            {
                return;
            }

            ReplaceHorizontalOrVerticalLayoutGroup<VerticalLayoutGroup>(horizontalLayoutGroup);
        }

        #endregion

        #region VerticalLayoutGroup

        [MenuItem(Context + "/" + nameof(VerticalLayoutGroup) + "/" + "Replace with " + nameof(HorizontalLayoutGroup))]
        private static void ReplaceVerticalLayoutGroupWithHorizontalLayoutGroup(MenuCommand menuCommand)
        {
            var verticalLayoutGroup = menuCommand.context as VerticalLayoutGroup;
            if (verticalLayoutGroup == null)
            {
                return;
            }

            ReplaceHorizontalOrVerticalLayoutGroup<HorizontalLayoutGroup>(verticalLayoutGroup);
        }

        #endregion
    }
}
