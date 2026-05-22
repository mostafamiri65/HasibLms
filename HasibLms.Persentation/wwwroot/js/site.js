// HasibLMS Main JavaScript File

// ============================================
// Global State
// ============================================
window.hasibLms = {
    init: function() {
        this.initComponents();
        this.initEventListeners();
        this.initLazyLoading();
    },

    initComponents: function() {
        // Initialize AOS animations
        if (typeof AOS !== 'undefined') {
            AOS.init({
                duration: 800,
                once: true,
                offset: 100
            });
        }

        // Initialize tooltips
        document.querySelectorAll('[data-tooltip]').forEach(el => {
            el.classList.add('tooltip');
            el.setAttribute('data-tooltip', el.getAttribute('data-tooltip'));
        });

        // Set current year in footer
        const yearElement = document.querySelector('[data-current-year]');
        if (yearElement) {
            yearElement.textContent = new Date().getFullYear();
        }
    },

    initEventListeners: function() {
        // Smooth scroll for anchor links
        document.querySelectorAll('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener('click', function(e) {
                const target = document.querySelector(this.getAttribute('href'));
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            });
        });

        // Back to top button
        const backToTop = document.querySelector('.back-to-top');
        if (backToTop) {
            window.addEventListener('scroll', () => {
                if (window.scrollY > 300) {
                    backToTop.classList.add('show');
                } else {
                    backToTop.classList.remove('show');
                }
            });

            backToTop.addEventListener('click', () => {
                window.scrollTo({ top: 0, behavior: 'smooth' });
            });
        }
    },

    initLazyLoading: function() {
        // Lazy load images
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    const src = img.getAttribute('data-src');
                    if (src) {
                        img.src = src;
                        img.classList.add('loaded');
                    }
                    observer.unobserve(img);
                }
            });
        });

        document.querySelectorAll('img[data-src]').forEach(img => {
            imageObserver.observe(img);
        });
    },

    // Toast notification
    showToast: function(message, type = 'success') {
        const container = document.getElementById('toastContainer');
        if (!container) return;

        const toast = document.createElement('div');
        const colors = {
            success: 'bg-green-500',
            error: 'bg-red-500',
            warning: 'bg-yellow-500',
            info: 'bg-blue-500'
        };
        const icons = {
            success: 'fa-check-circle',
            error: 'fa-exclamation-circle',
            warning: 'fa-exclamation-triangle',
            info: 'fa-info-circle'
        };
        toast.className = `toast-item ${colors[type]} text-white px-5 py-3 rounded-lg shadow-lg flex items-center gap-3`;
        toast.innerHTML = `
            <i class="fas ${icons[type]}"></i>
            <span>${message}</span>
        `;
        container.appendChild(toast);

        setTimeout(() => {
            toast.classList.add('show');
        }, 10);

        setTimeout(() => {
            toast.classList.remove('show');
            setTimeout(() => toast.remove(), 300);
        }, 3000);
    },

    // Modal handler
    openModal: function(modalId, data = null) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.remove('hidden');
            modal.classList.add('flex');
            document.body.style.overflow = 'hidden';

            if (data && typeof window.modalCallbacks === 'function') {
                window.modalCallbacks(data);
            }
        }
    },

    closeModal: function(modalId) {
        const modal = document.getElementById(modalId);
        if (modal) {
            modal.classList.add('hidden');
            modal.classList.remove('flex');
            document.body.style.overflow = '';
        }
    },

    // Form validation
    validateForm: function(form) {
        let isValid = true;
        const inputs = form.querySelectorAll('[required]');

        inputs.forEach(input => {
            if (!input.value.trim()) {
                isValid = false;
                input.classList.add('border-red-500');
                input.classList.add('input-validation-error');

                const errorMsg = input.getAttribute('data-error') || 'این فیلد الزامی است';
                let errorEl = input.parentElement?.querySelector('.field-validation-error');
                if (!errorEl) {
                    errorEl = document.createElement('span');
                    errorEl.className = 'field-validation-error text-red-500 text-xs mt-1 block';
                    errorEl.textContent = errorMsg;
                    input.parentElement?.appendChild(errorEl);
                }
            } else {
                input.classList.remove('border-red-500', 'input-validation-error');
                const errorEl = input.parentElement?.querySelector('.field-validation-error');
                if (errorEl) errorEl.remove();
            }
        });

        return isValid;
    },

    // Copy to clipboard
    copyToClipboard: function(text) {
        navigator.clipboard.writeText(text).then(() => {
            this.showToast('کپی شد!', 'success');
        }).catch(() => {
            this.showToast('خطا در کپی کردن', 'error');
        });
    },

    // Debounce function for search
    debounce: function(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }
};

// ============================================
// Cart functions
// ============================================
window.cartHandlers = {
    addToCart: function(courseId, courseTitle) {
        let cart = JSON.parse(localStorage.getItem('cart') || '[]');
        if (!cart.some(item => item.id === courseId)) {
            cart.push({ id: courseId, title: courseTitle });
            localStorage.setItem('cart', JSON.stringify(cart));
            hasibLms.showToast('دوره به سبد خرید اضافه شد', 'success');
            this.updateCartCount();
        } else {
            hasibLms.showToast('این دوره قبلاً در سبد خرید است', 'warning');
        }
    },

    removeFromCart: function(courseId) {
        let cart = JSON.parse(localStorage.getItem('cart') || '[]');
        cart = cart.filter(item => item.id !== courseId);
        localStorage.setItem('cart', JSON.stringify(cart));
        this.updateCartCount();
        hasibLms.showToast('دوره از سبد خرید حذف شد', 'info');
    },

    updateCartCount: function() {
        const cart = JSON.parse(localStorage.getItem('cart') || '[]');
        const cartCountElements = document.querySelectorAll('.cart-count');
        cartCountElements.forEach(el => {
            el.textContent = cart.length;
            el.classList.toggle('hidden', cart.length === 0);
        });
    },

    getCart: function() {
        return JSON.parse(localStorage.getItem('cart') || '[]');
    },

    clearCart: function() {
        localStorage.setItem('cart', '[]');
        this.updateCartCount();
    }
};

// ============================================
// Course enrollment
// ============================================
window.courseHandlers = {
    enroll: async function(courseId) {
        try {
            const response = await fetch(`/course/enroll`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value
                },
                body: JSON.stringify({ courseId: courseId })
            });

            const result = await response.json();

            if (result.success) {
                hasibLms.showToast('ثبت نام با موفقیت انجام شد', 'success');
                setTimeout(() => location.reload(), 1500);
            } else {
                hasibLms.showToast(result.message || 'خطا در ثبت نام', 'error');
            }
        } catch (error) {
            hasibLms.showToast('خطا در ارتباط با سرور', 'error');
        }
    }
};

// ============================================
// Rating stars component
// ============================================
class RatingStars {
    constructor(container, rating, readonly = false) {
        this.container = container;
        this.rating = rating;
        this.readonly = readonly;
        this.render();
    }

    render() {
        this.container.innerHTML = '';
        for (let i = 1; i <= 5; i++) {
            const star = document.createElement('i');
            star.className = `fas fa-star text-xl ${i <= this.rating ? 'text-yellow-400' : 'text-gray-300'} cursor-${this.readonly ? 'default' : 'pointer'} transition-colors`;

            if (!this.readonly) {
                star.addEventListener('click', () => this.setRating(i));
                star.addEventListener('mouseenter', () => this.hoverStar(i));
                star.addEventListener('mouseleave', () => this.resetStars());
            }

            this.container.appendChild(star);
        }
    }

    setRating(value) {
        this.rating = value;
        this.render();
        if (this.onChange) this.onChange(value);
    }

    hoverStar(value) {
        const stars = this.container.children;
        for (let i = 0; i < stars.length; i++) {
            stars[i].classList.toggle('text-yellow-400', i < value);
            stars[i].classList.toggle('text-gray-300', i >= value);
        }
    }

    resetStars() {
        this.render();
    }

    onChange(callback) {
        this.onChange = callback;
    }
}

// ============================================
// Initialize on DOM load
// ============================================
document.addEventListener('DOMContentLoaded', function() {
    hasibLms.init();
    window.cartHandlers.updateCartCount();

    // Initialize rating stars if exists
    document.querySelectorAll('.rating-stars').forEach(el => {
        const rating = parseInt(el.getAttribute('data-rating') || '0');
        const readonly = el.getAttribute('data-readonly') === 'true';
        new RatingStars(el, rating, readonly);
    });
});

// ============================================
// Alpine.js custom directives and store
// ============================================
document.addEventListener('alpine:init', () => {
    // Custom magic helper
    Alpine.magic('hasib', () => {
        return window.hasibLms;
    });

    // Store for global state
    Alpine.store('app', {
        darkMode: localStorage.getItem('darkMode') === 'true',
        user: null,
        cart: window.cartHandlers.getCart(),

        toggleDarkMode() {
            this.darkMode = !this.darkMode;
            localStorage.setItem('darkMode', this.darkMode);
            if (this.darkMode) {
                document.documentElement.classList.add('dark');
            } else {
                document.documentElement.classList.remove('dark');
            }
        },

        addToCart(courseId, courseTitle) {
            if (!this.cart.some(item => item.id === courseId)) {
                this.cart.push({ id: courseId, title: courseTitle });
                localStorage.setItem('cart', JSON.stringify(this.cart));
                hasibLms.showToast('دوره به سبد خرید اضافه شد', 'success');
            }
        }
    });
});

// ============================================
// Performance monitoring
// ============================================
if (typeof performance !== 'undefined') {
    window.addEventListener('load', () => {
        setTimeout(() => {
            const perfData = performance.timing;
            const pageLoadTime = perfData.loadEventEnd - perfData.navigationStart;
            console.log(`Page load time: ${pageLoadTime}ms`);
            if (pageLoadTime > 3000) {
                console.warn('Page load time is high:', pageLoadTime);
            }
        }, 0);
    });
}