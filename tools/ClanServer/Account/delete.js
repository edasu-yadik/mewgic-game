"use strict";

const form = document.getElementById("deletion");
const status = document.getElementById("result");
const button = form.querySelector("button");

function post(path, body, token) {
    const headers = { "Content-Type": "application/json" };
    if (token) headers.Authorization = "Bearer " + token;
    return fetch(path, { method: "POST", headers, body: JSON.stringify(body), credentials: "omit", redirect: "error" });
}

form.addEventListener("submit", async (event) => {
    event.preventDefault();
    if (button.disabled) return;
    const { handle, password } = Object.fromEntries(new FormData(form));
    button.disabled = true;
    status.textContent = "İşleniyor…";
    let session;
    try {
        const login = await post("/v1/login", { handle, password });
        if (!login.ok) {
            status.textContent = login.status === 429 ? "Çok fazla istek. Bir dakika sonra tekrar dene." :
                "Giriş yapılamadı. Bilgilerini kontrol et.";
            return;
        }
        session = await login.json();
        const deletion = await post("/v1/account/delete", { password }, session.token);
        if (!deletion.ok) {
            status.textContent = "Silme işlemi tamamlanamadı. Tekrar dene.";
            return;
        }
        session = null;
        form.reset();
        status.textContent = "Çevrimiçi hesabın silindi.";
    } catch {
        status.textContent = "Sunucuya bağlanılamadı. Tekrar dene.";
    } finally {
        if (session) post("/v1/session/revoke", { refreshToken: session.refreshToken }).catch(() => {});
        form.elements.password.value = "";
        button.disabled = false;
    }
});
