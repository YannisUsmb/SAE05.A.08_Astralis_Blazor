window.googleAuth = {
    initialize: (dotNetHelper, clientId) => {
        google.accounts.id.initialize({
            client_id: clientId,

            use_fedcm_for_prompt: false,
            ux_mode: "popup",

            callback: (response) => {
                console.log("Token Google reçu !");
                dotNetHelper.invokeMethodAsync('HandleGoogleLogin', response.credential);
            }
        });
    },
    renderButton: (elementId) => {
        const parent = document.getElementById(elementId);
        if (parent) {
            parent.innerHTML = '';
            google.accounts.id.renderButton(
                parent,
                {
                    theme: "filled_black",
                    size: "large",
                    width: "100%",
                    height: "52",
                    text: "continue_with",
                    shape: "pill"
                }
            );
        }
    }
};