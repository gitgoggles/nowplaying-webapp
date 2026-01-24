document.addEventListener("htmx:beforeSwap", (e) => {
  const incoming = (e.detail.serverResponse || "").trim();
  const current = (e.target.innerHTML || "").trim();

  if (incoming === current) {
    e.detail.shouldSwap = false;
    e.detail.isError = false;
  }
});
