using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MoonRabbitRush.Characters;
using MoonRabbitRush.UI;
using UnityEngine;
using UnityEngine.UI;

namespace MoonRabbitRush
{
    public class PopupSelectCharacter : UIPopup
    {
        private static readonly Color DisabledColor =
            new(0.5960784f, 0.5960784f, 0.5960784f, 1f);

        [SerializeField] private Transform _characterRect;
        [SerializeField] private CharacterSelectionCardView _cardPrefab;
        [SerializeField] private CharacterCatalog _characterCatalog;
        [SerializeField] private Button _selectButton;
        [SerializeField] private Image _selectButtonImage;

        private readonly List<CharacterSelectionCardView> _cards = new();
        private CancellationTokenSource _cancelToken;

        private CharacterSelectionCardView _selectedCard;

        private void OnEnable()
        {
            BuildCards();
            SelectCard(null);
        }

        public void OnClickStart()
        {
            if (_selectedCard == null || _cancelToken != null)
            {
                return;
            }

            CharacterSelectionSession.Select(_selectedCard.Character);

            var transitionCancellation = new CancellationTokenSource();
            _cancelToken = transitionCancellation;

            UniTask.Void(async () =>
            {
                try
                {
                    await ManagerRoot.Instance.SceneManager.TransitionTo(
                        1,
                        transitionCancellation.Token);
                }
                finally
                {
                    transitionCancellation.Dispose();
                    if (ReferenceEquals(_cancelToken, transitionCancellation))
                    {
                        _cancelToken = null;
                    }
                }
            });
        }

        private void BuildCards()
        {
            ClearCards();

            if (_characterRect == null || _cardPrefab == null)
            {
                Debug.LogError("Character selection UI references are missing.", this);
                return;
            }

            if (_characterCatalog == null)
            {
                Debug.LogError("Character catalog is not assigned.", this);
                return;
            }

            foreach (CharacterData character in _characterCatalog.Characters)
            {
                if (character == null || !character.IsValid)
                {
                    continue;
                }

                CharacterSelectionCardView card = Instantiate(
                    _cardPrefab,
                    _characterRect);
                card.Bind(character, HandleCardClicked);
                _cards.Add(card);
            }
        }

        private void ClearCards()
        {
            if (_characterRect != null)
            {
                CharacterSelectionCardView[] existingCards =
                    _characterRect.GetComponentsInChildren<
                        CharacterSelectionCardView>(true);

                foreach (CharacterSelectionCardView card in existingCards)
                {
                    card.gameObject.SetActive(false);
                    Destroy(card.gameObject);
                }
            }

            _cards.Clear();
            _selectedCard = null;
        }

        private void HandleCardClicked(CharacterSelectionCardView card)
        {
            SelectCard(ReferenceEquals(_selectedCard, card) ? null : card);
        }

        private void SelectCard(CharacterSelectionCardView card)
        {
            _selectedCard = card;

            foreach (CharacterSelectionCardView item in _cards)
            {
                item.SetSelected(ReferenceEquals(item, card));
            }

            bool hasSelection = card != null;
            if (_selectButton != null)
            {
                _selectButton.interactable = hasSelection;
            }

            if (_selectButtonImage != null)
            {
                _selectButtonImage.color = hasSelection
                    ? Color.white
                    : DisabledColor;
            }
        }
    }
}
