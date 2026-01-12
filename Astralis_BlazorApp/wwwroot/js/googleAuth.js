window.googleAuth = {
    initialize: (dotNetHelper, clientId) => {
        google.accounts.id.initialize({
            client_id: clientId,
            use_fedcm_for_prompt: false,
            ux_mode: "popup",
            callback: (response) => {
                dotNetHelper.invokeMethodAsync('HandleGoogleLogin', response.credential);
            }
        });
    },
    renderButton: (elementId) => {
        const parent = document.getElementById(elementId);
        if (parent) {
            const width = parent.clientWidth || 300;

            parent.innerHTML = '';
            google.accounts.id.renderButton(
                parent,
                {
                    theme: "outline",
                    size: "large",
                    type: "standard",
                    shape: "rectangular",
                    width: width,
                    logo_alignment: "left"
                }
            );
        }
    }
};