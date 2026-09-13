(() => {
  const checks = [...document.querySelectorAll('[data-step]')];
  const bar = document.querySelector('#progress-bar');
  const label = document.querySelector('#progress-label');
  const key = 'flowbridge-launch-progress-v1';

  function update() {
    const done = checks.filter((check) => check.checked).length;
    bar.style.width = `${(done / checks.length) * 100}%`;
    label.textContent = `${done} of ${checks.length} launch steps complete`;
    localStorage.setItem(key, JSON.stringify(checks.filter((check) => check.checked).map((check) => check.dataset.step)));
  }

  try {
    const saved = JSON.parse(localStorage.getItem(key) || '[]');
    checks.forEach((check) => { check.checked = saved.includes(check.dataset.step); check.addEventListener('change', update); });
  } catch { checks.forEach((check) => check.addEventListener('change', update)); }
  update();

  const serviceGrid = document.querySelector('[data-service-grid]');
  const escapeHtml = (value) => String(value ?? '').replace(/[&<>'"]/g, (character) => ({
    '&': '&amp;', '<': '&lt;', '>': '&gt;', "'": '&#39;', '"': '&quot;'
  })[character]);

  async function loadDatabaseServices() {
    if (!serviceGrid) return;

    try {
      const response = await fetch('/api/services', { headers: { Accept: 'application/json' }, cache: 'no-store' });
      if (!response.ok) return;

      const services = await response.json();
      if (!Array.isArray(services) || services.length === 0) return;

      serviceGrid.innerHTML = services.map((service, index) => {
        const bullets = Array.isArray(service.bulletPoints) ? service.bulletPoints : [];
        return `<article class="service-card tone-${(index % 7) + 1}">
          <p class="card-label">${escapeHtml(service.practice || `PRACTICE ${String(index + 1).padStart(2, '0')}`)}</p>
          <h3>${escapeHtml(service.title)}</h3>
          <p>${escapeHtml(service.summary)}</p>
          <ul>${bullets.map((bullet) => `<li>${escapeHtml(bullet)}</li>`).join('')}</ul>
          <p class="fit"><b>Best fit:</b> ${escapeHtml(service.audience)}</p>
        </article>`;
      }).join('');
    } catch {
      // The published static site intentionally keeps its authored fallback cards.
    }
  }

  loadDatabaseServices();
})();

