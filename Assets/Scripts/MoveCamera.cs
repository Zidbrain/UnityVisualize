using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;
using UnityEngine.InputSystem;

namespace Assets.Scripts
{
    public class MoveCamera : MonoBehaviour
    {
        private Vector3 _centerPosition = Vector3.zero;
        private const float s_baseElevation = 575.745f;

        private Vector2 _yawPitch = new Vector2(90f, 0f);

        public float speed;
        public float baseElevation;

        private Keyboard keyboard = Keyboard.current;
        private Mouse mouse = Mouse.current;
        private Vector2 oldMousePos;

        private void Update()
        {   
            if (keyboard.fKey.wasReleasedThisFrame) Screen.fullScreen = !Screen.fullScreen;

            var vector = new Vector3();

            if (keyboard.aKey.isPressed) vector += new Vector3(-1f, 0f, 0f);
            if (keyboard.dKey.isPressed) vector += new Vector3(1f, 0f, 0f);
            if (keyboard.wKey.isPressed) vector += new Vector3(0f, 0f, 1f);
            if (keyboard.sKey.isPressed) vector += new Vector3(0f, 0f, -1f);

            var input = Vector2.zero;

            if (keyboard.leftArrowKey.isPressed) input += new Vector2(-1f, 0f);
            if (keyboard.rightArrowKey.isPressed) input += new Vector2(1f, 0f);
            if (keyboard.upArrowKey.isPressed) input += new Vector2(0f, 1f);
            if (keyboard.downArrowKey.isPressed) input += new Vector2(0f, -1f);

            input *= 0.7f;

            if (keyboard.spaceKey.wasPressedThisFrame)
            {
                oldMousePos = mouse.position.ReadValue();
            }

            if (keyboard.spaceKey.isPressed)
            {
                UnityEngine.Cursor.visible = false;
                input = (mouse.position.ReadValue() - oldMousePos).FlipY() * 0.35f;
                mouse.WarpCursorPosition(oldMousePos);
            }
            else
                UnityEngine.Cursor.visible = true;

            const float e = 0.001f;
            if (input.x < -e || input.x > e || input.y < -e || input.y > e)
            {
                _yawPitch += 100f * Time.unscaledDeltaTime * new Vector2(input.y, input.x);
            }

            var scroll = mouse.scroll.ReadValue() / 120f;
            baseElevation /= Mathf.Exp(scroll.y * 0.1f);

            _centerPosition += baseElevation * speed * Time.deltaTime *
             (Quaternion.Euler(0f, _yawPitch.y, 0f) * vector.normalized) /
                s_baseElevation * (keyboard.leftShiftKey.isPressed ? 2f : 1f);

            var lookRotation = Quaternion.Euler(_yawPitch);
            var lookDirection = lookRotation * Vector3.forward;
            var lookPosition = _centerPosition - lookDirection * baseElevation;

            transform.SetPositionAndRotation(lookPosition, lookRotation);
        }
    }
}
