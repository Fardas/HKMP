using HKMP.Game;
using HKMP.Game.Client;
using HKMP.UI.Component;
using UnityEngine;

namespace HKMP.UI {
    public class ClientSettingsUI {
        private readonly ClientManager _clientManager;
        
        private readonly GameObject _settingsUiObject;
        private readonly GameObject _connectUiObject;

        public ClientSettingsUI(
            ClientManager clientManager,
            GameObject settingsUiObject, 
            GameObject connectUiObject
        ) {
            _clientManager = clientManager;
            
            _settingsUiObject = settingsUiObject;
            _connectUiObject = connectUiObject;
            
            CreateSettingsUI();
        }

        private void CreateSettingsUI() {
            _settingsUiObject.SetActive(false);

            var x = Screen.width - 210f;
            var y = Screen.height - 75f;

            var radioButtonBox = new RadioButtonBoxComponent(
                _settingsUiObject,
                new Vector2(x, y),
                new Vector2(300, 35),
                new[] {
                    "No team",
                    "Red",
                    "Blue",
                    "Yellow",
                    "Green"
                },
                0
            );

            y -= 200;

            new ButtonComponent(
                _settingsUiObject,
                new Vector2(x, y),
                "Back"
            ).SetOnPress(() => {
                _settingsUiObject.SetActive(false);
                _connectUiObject.SetActive(true);
            });
            
            _clientManager.RegisterOnConnect(() => radioButtonBox.SetActiveIndex(0));
            
            radioButtonBox.SetOnChange(value => {
                _clientManager.ChangeTeam((Team) value);
            });
        }
    }
}