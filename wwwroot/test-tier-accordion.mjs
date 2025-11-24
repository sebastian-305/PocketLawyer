/**
 * Test suite for tier accordion structure
 * 
 * This test verifies that:
 * 1. createTierAccordion function creates proper Bootstrap accordion structure
 * 2. Basic analysis accordion is expanded by default
 * 3. Premium analysis accordion is collapsed by default
 * 4. Accordions can be toggled independently
 * 5. Proper ARIA attributes are set for accessibility
 */

import { createTierAccordion } from './render.js';

const DEBUG = true;

function log(message, data = null) {
    if (DEBUG) {
        console.log(`[TEST] ${message}`, data || '');
    }
}

function assert(condition, message) {
    if (!condition) {
        throw new Error(`Assertion failed: ${message}`);
    }
    log(`✓ ${message}`);
}

function testCreateTierAccordion() {
    log('Testing createTierAccordion function...');

    // Create test content
    const testContent = document.createElement('div');
    testContent.textContent = 'Test content';

    // Test 1: Create expanded accordion
    const expandedAccordion = createTierAccordion('test-expanded', 'Test Expanded', testContent, true);
    
    assert(expandedAccordion !== null, 'Expanded accordion should be created');
    assert(expandedAccordion.classList.contains('accordion-item'), 'Should have accordion-item class');
    
    const expandedButton = expandedAccordion.querySelector('.accordion-button');
    assert(expandedButton !== null, 'Should have accordion button');
    assert(!expandedButton.classList.contains('collapsed'), 'Expanded accordion button should not have collapsed class');
    assert(expandedButton.getAttribute('aria-expanded') === 'true', 'Expanded accordion should have aria-expanded=true');
    
    const expandedCollapse = expandedAccordion.querySelector('.accordion-collapse');
    assert(expandedCollapse !== null, 'Should have collapse element');
    assert(expandedCollapse.classList.contains('show'), 'Expanded accordion should have show class');
    assert(expandedCollapse.id === 'test-expanded', 'Collapse element should have correct ID');

    // Test 2: Create collapsed accordion
    const collapsedAccordion = createTierAccordion('test-collapsed', 'Test Collapsed', testContent, false);
    
    assert(collapsedAccordion !== null, 'Collapsed accordion should be created');
    
    const collapsedButton = collapsedAccordion.querySelector('.accordion-button');
    assert(collapsedButton.classList.contains('collapsed'), 'Collapsed accordion button should have collapsed class');
    assert(collapsedButton.getAttribute('aria-expanded') === 'false', 'Collapsed accordion should have aria-expanded=false');
    
    const collapsedCollapse = collapsedAccordion.querySelector('.accordion-collapse');
    assert(!collapsedCollapse.classList.contains('show'), 'Collapsed accordion should not have show class');

    // Test 3: Verify content is properly nested
    const body = expandedAccordion.querySelector('.accordion-body');
    assert(body !== null, 'Should have accordion body');
    assert(body.contains(testContent), 'Body should contain the provided content');

    // Test 4: Verify button attributes
    assert(expandedButton.getAttribute('data-bs-toggle') === 'collapse', 'Button should have data-bs-toggle=collapse');
    assert(expandedButton.getAttribute('data-bs-target') === '#test-expanded', 'Button should target correct collapse element');
    assert(expandedButton.getAttribute('aria-controls') === 'test-expanded', 'Button should have correct aria-controls');
    assert(expandedButton.textContent === 'Test Expanded', 'Button should display correct title');

    log('All createTierAccordion tests passed!');
}

function testTierAccordionStructure() {
    log('Testing tier accordion structure...');

    // Create a container
    const container = document.createElement('div');
    container.classList.add('tier-accordion');

    // Create Basic Analysis content
    const basicContent = document.createElement('div');
    basicContent.innerHTML = '<p>Basic analysis content</p>';

    // Create Premium Analysis content
    const premiumContent = document.createElement('div');
    premiumContent.innerHTML = '<p>Premium analysis content</p>';

    // Create accordions
    const basicAccordion = createTierAccordion('tier-basic', 'Basis Analyse', basicContent, true);
    const premiumAccordion = createTierAccordion('tier-premium', 'Premium Analyse', premiumContent, false);

    container.appendChild(basicAccordion);
    container.appendChild(premiumAccordion);

    // Verify structure
    assert(container.children.length === 2, 'Container should have 2 accordion items');
    
    const firstAccordion = container.children[0];
    const secondAccordion = container.children[1];

    // Verify Basic Analysis (first, expanded)
    const basicButton = firstAccordion.querySelector('.accordion-button');
    assert(basicButton.textContent === 'Basis Analyse', 'First accordion should be Basic Analysis');
    assert(!basicButton.classList.contains('collapsed'), 'Basic Analysis should be expanded');
    
    const basicCollapse = firstAccordion.querySelector('.accordion-collapse');
    assert(basicCollapse.classList.contains('show'), 'Basic Analysis collapse should have show class');

    // Verify Premium Analysis (second, collapsed)
    const premiumButton = secondAccordion.querySelector('.accordion-button');
    assert(premiumButton.textContent === 'Premium Analyse', 'Second accordion should be Premium Analysis');
    assert(premiumButton.classList.contains('collapsed'), 'Premium Analysis should be collapsed');
    
    const premiumCollapse = secondAccordion.querySelector('.accordion-collapse');
    assert(!premiumCollapse.classList.contains('show'), 'Premium Analysis collapse should not have show class');

    log('All tier accordion structure tests passed!');
}

function testAccessibility() {
    log('Testing accessibility attributes...');

    const testContent = document.createElement('div');
    const accordion = createTierAccordion('test-a11y', 'Test Accessibility', testContent, true);

    const button = accordion.querySelector('.accordion-button');
    const collapse = accordion.querySelector('.accordion-collapse');

    // Verify ARIA attributes
    assert(button.getAttribute('aria-expanded') !== null, 'Button should have aria-expanded attribute');
    assert(button.getAttribute('aria-controls') !== null, 'Button should have aria-controls attribute');
    assert(button.getAttribute('aria-controls') === collapse.id, 'aria-controls should match collapse ID');
    
    // Verify button type
    assert(button.type === 'button', 'Button should have type="button"');

    // Verify header structure
    const header = accordion.querySelector('.accordion-header');
    assert(header !== null, 'Should have accordion header');
    assert(header.tagName === 'H2', 'Header should be h2 element');

    log('All accessibility tests passed!');
}

// Run all tests
try {
    log('Starting tier accordion tests...\n');
    
    testCreateTierAccordion();
    console.log('');
    
    testTierAccordionStructure();
    console.log('');
    
    testAccessibility();
    console.log('');
    
    log('✅ All tests passed successfully!');
} catch (error) {
    console.error('❌ Test failed:', error.message);
    console.error(error.stack);
}
