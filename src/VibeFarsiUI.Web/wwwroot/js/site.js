// VibeFarsiUI theme persistence and small clipboard helper.
(function () {
  const KEY = "vibefarsi-theme";

  function normalize(name) {
    return !name || name === "graphite" ? "" : name;
  }

  function apply(name) {
    const root = document.documentElement;
    const value = normalize(name);
    if (value) root.setAttribute("data-theme", value);
    else root.removeAttribute("data-theme");
  }

  window.vfTheme = {
    init() {
      try {
        apply(localStorage.getItem(KEY) || "");
      } catch (e) {
        apply("");
      }
    },
    set(name) {
      apply(name);
      try {
        if (!name || name === "graphite") localStorage.removeItem(KEY);
        else localStorage.setItem(KEY, name);
      } catch (e) {
        /* storage disabled */
      }
      window.dispatchEvent(new CustomEvent("vf-theme-changed", { detail: name || "graphite" }));
    },
    get() {
      try {
        return localStorage.getItem(KEY) || "graphite";
      } catch (e) {
        return "graphite";
      }
    },
  };

  window.vfFocus = function (id) {
    const element = document.getElementById(id);
    if (!element) return false;
    element.focus();
    if (element.select) element.select();
    return true;
  };

  window.vfCopy = async function (text) {
    try {
      await navigator.clipboard.writeText(text);
      return true;
    } catch (e) {
      return false;
    }
  };

  document.addEventListener("DOMContentLoaded", function () {
    window.vfTheme.init();
  });
})();
