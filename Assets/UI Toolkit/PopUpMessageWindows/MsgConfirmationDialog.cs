using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace MessageWindow
{
    [UxmlElement]
    public partial class MsgConfirmationDialog : VisualElement
{
    private const string styleResource = "USS/MessageWindowStyleSheet";
    private const string ussMessageWindow = "message_window";
    private const string ussMessageContainer = "message_container";
    private const string ussHorContainer = "horizontal_container";
    private const string ussMessageLabel = "message_label";
    private const string ussMessageButton = "message_button";
    private const string ussBtnCancel = "btn_cancel";
    private const string ussBtnConfirm = "btn_confirm";

    [UxmlAttribute]
    public string LabelText { get; set; } = "Are you sure you want to delete this item?";

    [UxmlAttribute]
    public string CancelButtonText { get; set; } = "No";

    [UxmlAttribute]
    public string ConfirmButtonText { get; set; } = "Yes";

    // New constructor
    public MsgConfirmationDialog(string labelString, Action onYes, Action onNo)
    {
        LabelText = labelString;

        // Subscribe the actions to the confirm and cancel events
        confirmed += onYes;
        cancelled += onNo;

        Initialize();
    }

    public MsgConfirmationDialog()
    {
        Initialize();
    }

    private void Initialize()
    {
        styleSheets.Add(Resources.Load<StyleSheet>(styleResource));
        AddToClassList(ussMessageContainer);

        // Create window
        VisualElement window = new VisualElement();
        window.AddToClassList(ussMessageWindow);
        Add(window); // Directly add to current element

        // Text part
        VisualElement horizontalContainerText = new VisualElement();
        horizontalContainerText.AddToClassList(ussHorContainer);
        window.Add(horizontalContainerText);

        Label messageLabel = new Label() { text = LabelText };
        messageLabel.AddToClassList(ussMessageLabel);
        horizontalContainerText.Add(messageLabel);

        // Button part
        VisualElement horizontalContainerButton = new VisualElement();
        horizontalContainerButton.AddToClassList(ussHorContainer);
        window.Add(horizontalContainerButton);

        Button btnCancel = new Button() { text = CancelButtonText };
        btnCancel.AddToClassList(ussMessageButton);
        btnCancel.AddToClassList(ussBtnCancel);
        btnCancel.clicked += OnCancel;
        horizontalContainerButton.Add(btnCancel);

        Button btnConfirm = new Button() { text = ConfirmButtonText };
        btnConfirm.AddToClassList(ussMessageButton);
        btnConfirm.AddToClassList(ussBtnConfirm);
        btnConfirm.clicked += OnConfirm;
        horizontalContainerButton.Add(btnConfirm);
    }

    public event Action confirmed;
    public event Action cancelled;

    private void OnConfirm() => confirmed?.Invoke();
    private void OnCancel() => cancelled?.Invoke();
}
}