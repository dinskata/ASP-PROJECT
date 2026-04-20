document.addEventListener("DOMContentLoaded", () => {
  const hero = document.querySelector(".hero-section");
  if (hero) {
    hero.animate(
      [
        { opacity: 0, transform: "translateY(16px)" },
        { opacity: 1, transform: "translateY(0)" }
      ],
      { duration: 700, easing: "ease-out", fill: "forwards" }
    );
  }
});
