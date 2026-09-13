(() => {
  const editors = document.querySelector('#service-editors');
  const status = document.querySelector('#admin-status');
  const name = document.querySelector('#admin-name');
  const addButton = document.querySelector('#add-service-button');
  const logoutButton = document.querySelector('#logout-button');
  let token = '';

  const blankService = () => ({ practice: 'NEW SERVICE', title: '', summary: '', bulletPoints: '', audience: '', isPublished: false, sortOrder: 60 });
  const say = (message, error = false) => { status.textContent = message; status.classList.toggle('error', error); };

  async function requestToken() {
    const response = await fetch('/api/admin/antiforgery', { cache: 'no-store' });
    if (!response.ok) throw new Error('Could not prepare a secure content request.');
    token = (await response.json()).token;
  }

  async function api(url, options = {}) {
    const headers = { ...(options.headers || {}) };
    if (options.method && options.method !== 'GET') headers['X-CSRF-TOKEN'] = token;
    const response = await fetch(url, { ...options, headers });
    if (response.status === 401 || response.status === 403) {
      window.location.assign('/admin/login.html');
      throw new Error('Your administrator session has ended.');
    }
    if (!response.ok) {
      const problem = await response.json().catch(() => ({}));
      const detail = problem.detail || Object.values(problem.errors || {}).flat().join(' ') || 'The change could not be saved.';
      throw new Error(detail);
    }
    return response.status === 204 ? null : response.json();
  }

  function field(label, key, value, type = 'text', full = false) {
    const wrapper = document.createElement('label');
    if (full) wrapper.className = 'full';
    wrapper.append(document.createTextNode(label));
    const control = type === 'textarea' ? document.createElement('textarea') : document.createElement('input');
    control.name = key;
    if (type !== 'textarea') control.type = type;
    control.value = value ?? '';
    wrapper.append(control);
    return wrapper;
  }

  function editor(service) {
    const form = document.createElement('form');
    form.className = 'editor';
    form.dataset.id = service.id || '';
    const head = document.createElement('div');
    head.className = 'editor-head';
    const heading = document.createElement('h2');
    heading.textContent = service.title || 'New service';
    const id = document.createElement('span');
    id.className = 'editor-id';
    id.textContent = service.id ? `ID ${service.id}` : 'NOT SAVED';
    head.append(heading, id);
    const grid = document.createElement('div');
    grid.className = 'editor-grid';
    grid.append(field('Practice label', 'practice', service.practice));
    grid.append(field('Display order', 'sortOrder', service.sortOrder, 'number'));
    grid.append(field('Service title', 'title', service.title, 'text', true));
    grid.append(field('Summary', 'summary', service.summary, 'textarea', true));
    grid.append(field('Bullet points — one per line', 'bulletPoints', service.bulletPoints, 'textarea', true));
    grid.append(field('Best-fit audience', 'audience', service.audience, 'textarea', true));
    const toggle = document.createElement('label');
    toggle.className = 'toggle full';
    const published = document.createElement('input');
    published.type = 'checkbox';
    published.name = 'isPublished';
    published.checked = Boolean(service.isPublished);
    toggle.append(published, document.createTextNode('Publish this service on the public site'));
    grid.append(toggle);
    const actions = document.createElement('div');
    actions.className = 'editor-actions';
    const save = document.createElement('button');
    save.type = 'submit';
    save.textContent = service.id ? 'Save changes' : 'Create service';
    actions.append(save);
    if (service.id) {
      const remove = document.createElement('button');
      remove.type = 'button';
      remove.className = 'delete-button';
      remove.textContent = 'Delete service';
      remove.addEventListener('click', async () => {
        if (!window.confirm(`Delete “${service.title}”? This cannot be undone from the dashboard.`)) return;
        try {
          await api(`/api/admin/services/${service.id}`, { method: 'DELETE' });
          say('Service deleted.');
          await loadServices();
        } catch (error) { say(error.message, true); }
      });
      actions.append(remove);
    }
    form.append(head, grid, actions);
    form.addEventListener('submit', async (event) => {
      event.preventDefault();
      const values = new FormData(form);
      const payload = {
        practice: values.get('practice'), title: values.get('title'), summary: values.get('summary'),
        bulletPoints: values.get('bulletPoints'), audience: values.get('audience'),
        isPublished: values.get('isPublished') === 'on', sortOrder: Number(values.get('sortOrder'))
      };
      try {
        const path = service.id ? `/api/admin/services/${service.id}` : '/api/admin/services';
        const method = service.id ? 'PUT' : 'POST';
        await api(path, { method, headers: { 'Content-Type': 'application/json' }, body: JSON.stringify(payload) });
        say('Service saved to the local database.');
        await loadServices();
      } catch (error) { say(error.message, true); }
    });
    return form;
  }

  async function loadServices() {
    const services = await api('/api/admin/services');
    editors.replaceChildren(...services.map(editor));
  }

  async function start() {
    try {
      const session = await api('/api/admin/session');
      name.textContent = session.name;
      await requestToken();
      await loadServices();
    } catch (error) {
      if (!error.message.includes('session has ended')) say(error.message, true);
    }
  }

  addButton.addEventListener('click', () => editors.prepend(editor(blankService())));
  logoutButton.addEventListener('click', async () => {
    try {
      await api('/api/admin/logout', { method: 'POST' });
      window.location.assign('/admin/login.html');
    } catch (error) { say(error.message, true); }
  });
  start();
})();
