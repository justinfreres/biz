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

  const consultationForm = document.querySelector('[data-consultation-form]');
  const consultationDate = document.querySelector('#consultation-date');
  const consultationStatus = document.querySelector('#consultation-status');

  if (consultationDate) {
    consultationDate.min = new Date().toISOString().slice(0, 10);
  }

  if (consultationForm) {
    consultationForm.addEventListener('submit', (event) => {
      event.preventDefault();
      const request = new FormData(consultationForm);
      const name = request.get('name') || '';
      const service = request.get('service') || 'Custom consultation';
      const duration = request.get('duration') || '30 minutes';
      const lines = [
        'FlowBridge paid consultation request',
        '',
        `Name: ${name}`,
        `Work email: ${request.get('email') || ''}`,
        `Company: ${request.get('company') || ''}`,
        `Phone: ${request.get('phone') || ''}`,
        `Primary need: ${service}`,
        `Requested time: ${duration}`,
        `Preferred day: ${request.get('date') || ''}`,
        '',
        'Desired outcome:',
        String(request.get('details') || ''),
        '',
        'Manual payment: $50 initial booking fee. Confirm remaining scope and availability before reserving the session.'
      ];
      const subject = `Paid consultation request | ${service} | ${duration}`;
      const leadGateway = 'info@flowbridge-systems-llc.odoo.com';

      if (consultationStatus) {
        consultationStatus.textContent = 'Opening your email application. Send the message to create your Odoo CRM consultation request.';
      }

      window.location.href = `mailto:${leadGateway}?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(lines.join('\n'))}`;
    });
  }

  const productOrderForm = document.querySelector('[data-product-order-form]');
  const productOrderStatus = document.querySelector('#product-order-status');

  if (productOrderForm) {
    productOrderForm.addEventListener('submit', (event) => {
      event.preventDefault();
      const request = new FormData(productOrderForm);
      const productTitle = productOrderForm.dataset.productTitle || 'FlowBridge digital product';
      const price = productOrderForm.dataset.productPrice || '$5.99';
      const lines = [
        'FlowBridge digital product order request',
        '',
        `Product: ${productTitle}`,
        `Price: ${price} USD`,
        `Name: ${request.get('name') || ''}`,
        `Delivery email: ${request.get('email') || ''}`,
        '',
        'Customer note:',
        String(request.get('note') || ''),
        '',
        'Manual fulfillment: send payment instructions, confirm payment, then email the digital PDF.'
      ];
      const subject = `Digital product order | ${productTitle} | ${price}`;
      const leadGateway = 'info@flowbridge-systems-llc.odoo.com';

      if (productOrderStatus) {
        productOrderStatus.textContent = 'Opening your email application. Send the message to create your Odoo CRM digital-order request.';
      }

      window.location.href = `mailto:${leadGateway}?subject=${encodeURIComponent(subject)}&body=${encodeURIComponent(lines.join('\n'))}`;
    });
  }
})();
