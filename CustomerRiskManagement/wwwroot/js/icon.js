document.addEventListener("DOMContentLoaded", function () {
    initIcons();
});


function initIcons() {
    document.querySelectorAll("icon").forEach(icon => {
        const name = icon.getAttribute("name");
        const cls = icon.getAttribute("class") ?? "";

        fetch(`/icons/unColored/${name}.svg`)
            .then(res => res.text())
            .then(svg => {
                const wrapper = document.createElement("span"); // inline konteyner
                wrapper.innerHTML = svg;
                const svgEl = wrapper.querySelector("svg");
                if (svgEl) {
                    svgEl.setAttribute("class", cls);
                    svgEl.setAttribute("fill", "currentColor");
                    svgEl.style.display = "inline-block"; // metin ile aynı hizada
                    svgEl.style.verticalAlign = "middle";
                }


                icon.replaceWith(wrapper); // span ile değiştir
            });
    });
}